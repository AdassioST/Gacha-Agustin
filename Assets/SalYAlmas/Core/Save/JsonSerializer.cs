using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using SalYAlmas.Core.Data;

namespace SalYAlmas.Core.Save
{
    /// <summary>El serializador no guarda este campo (estado derivado o referencias a datos).</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SaveIgnoreAttribute : Attribute { }

    /// <summary>
    /// Serializador JSON por reflexión para el perfil del jugador. Guarda los campos públicos de instancia
    /// (en orden de declaración), listas, arreglos, conjuntos, diccionarios con clave de texto, enums por nombre,
    /// nulables y clases anidadas. Los ulong van como texto para no perder precisión (estado del azar).
    /// La salida es determinista: los diccionarios y conjuntos se ordenan por clave.
    /// </summary>
    public static class JsonSerializer
    {
        public static string ToJson(object value, bool pretty = true)
        {
            var sb = new StringBuilder();
            Write(sb, value, pretty, 0);
            return sb.ToString();
        }

        public static T FromJson<T>(string json) where T : class, new()
        {
            var node = Json.Parse(json, typeof(T).Name);
            return (T)Read(node, typeof(T), null);
        }

        public static void Populate(object target, string json)
        {
            var node = Json.Parse(json, target.GetType().Name);
            ReadInto(node, target);
        }

        // ------------------------------------------------------------------ Escritura

        private static void Indent(StringBuilder sb, bool pretty, int depth)
        {
            if (!pretty) return;
            sb.Append('\n');
            sb.Append(' ', depth * 2);
        }

        private static void Write(StringBuilder sb, object v, bool pretty, int depth)
        {
            if (v == null) { sb.Append("null"); return; }
            var t = v.GetType();
            switch (v)
            {
                case string s: WriteString(sb, s); return;
                case bool b: sb.Append(b ? "true" : "false"); return;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); return;
                case long l: sb.Append(l.ToString(CultureInfo.InvariantCulture)); return;
                case ulong u: WriteString(sb, u.ToString(CultureInfo.InvariantCulture)); return;
                case double d: sb.Append(FormatDouble(d)); return;
                case float f: sb.Append(FormatDouble(f)); return;
            }
            if (t.IsEnum) { WriteString(sb, v.ToString()); return; }

            if (v is IDictionary dict)
            {
                var keys = new List<string>();
                foreach (var k in dict.Keys) keys.Add(Convert.ToString(k, CultureInfo.InvariantCulture));
                keys.Sort(StringComparer.Ordinal);
                var byString = new Dictionary<string, object>();
                foreach (DictionaryEntry e in dict) byString[Convert.ToString(e.Key, CultureInfo.InvariantCulture)] = e.Value;
                sb.Append('{');
                for (int i = 0; i < keys.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    Indent(sb, pretty, depth + 1);
                    WriteString(sb, keys[i]);
                    sb.Append(pretty ? ": " : ":");
                    Write(sb, byString[keys[i]], pretty, depth + 1);
                }
                if (keys.Count > 0) Indent(sb, pretty, depth);
                sb.Append('}');
                return;
            }

            if (v is IEnumerable seq)
            {
                var items = new List<object>();
                foreach (var item in seq) items.Add(item);
                if (IsSet(t)) items.Sort((a, b) => string.CompareOrdinal(Convert.ToString(a, CultureInfo.InvariantCulture), Convert.ToString(b, CultureInfo.InvariantCulture)));
                bool simple = items.TrueForAll(x => x == null || x is string || x.GetType().IsPrimitive || x.GetType().IsEnum);
                sb.Append('[');
                for (int i = 0; i < items.Count; i++)
                {
                    if (i > 0) sb.Append(simple && pretty ? ", " : ",");
                    if (!simple) Indent(sb, pretty, depth + 1);
                    Write(sb, items[i], pretty, depth + 1);
                }
                if (!simple && items.Count > 0) Indent(sb, pretty, depth);
                sb.Append(']');
                return;
            }

            var fields = FieldsOf(t);
            sb.Append('{');
            bool first = true;
            foreach (var f in fields)
            {
                if (!first) sb.Append(',');
                first = false;
                Indent(sb, pretty, depth + 1);
                WriteString(sb, f.Name);
                sb.Append(pretty ? ": " : ":");
                Write(sb, f.GetValue(v), pretty, depth + 1);
            }
            if (!first) Indent(sb, pretty, depth);
            sb.Append('}');
        }

        private static string FormatDouble(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) return "0";
            return d.ToString("R", CultureInfo.InvariantCulture);
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new Dictionary<Type, FieldInfo[]>();

        private static FieldInfo[] FieldsOf(Type t)
        {
            lock (FieldCache)
            {
                if (FieldCache.TryGetValue(t, out var cached)) return cached;
                var list = new List<FieldInfo>();
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    if (f.GetCustomAttribute<SaveIgnoreAttribute>() == null) list.Add(f);
                list.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
                var arr = list.ToArray();
                FieldCache[t] = arr;
                return arr;
            }
        }

        private static bool IsSet(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(HashSet<>);

        // ------------------------------------------------------------------ Lectura

        private static object Read(JsonNode n, Type t, object existing)
        {
            if (n == null || n.Kind == JsonKind.Null)
            {
                if (t.IsValueType && Nullable.GetUnderlyingType(t) == null) return Activator.CreateInstance(t);
                return null;
            }
            var under = Nullable.GetUnderlyingType(t);
            if (under != null) t = under;

            if (t == typeof(string)) return n.Kind == JsonKind.String ? n.AsString : Convert.ToString(n.AsNumber, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return n.AsBool;
            if (t == typeof(int)) return (int)Math.Round(n.AsNumber);
            if (t == typeof(long)) return (long)Math.Round(n.AsNumber);
            if (t == typeof(double)) return n.AsNumber;
            if (t == typeof(float)) return (float)n.AsNumber;
            if (t == typeof(ulong))
                return n.Kind == JsonKind.String ? ulong.Parse(n.AsString, CultureInfo.InvariantCulture) : (ulong)n.AsNumber;
            if (t.IsEnum) return Enum.Parse(t, n.AsString);

            if (t.IsArray)
            {
                var et = t.GetElementType();
                var arr = Array.CreateInstance(et, n.Items.Count);
                for (int i = 0; i < n.Items.Count; i++) arr.SetValue(Read(n.Items[i], et, null), i);
                return arr;
            }

            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                var args = t.GetGenericArguments();
                if (def == typeof(List<>) || def == typeof(HashSet<>))
                {
                    var coll = existing ?? Activator.CreateInstance(t);
                    t.GetMethod("Clear").Invoke(coll, null);
                    var add = t.GetMethod("Add");
                    foreach (var item in n.Items) add.Invoke(coll, new[] { Read(item, args[0], null) });
                    return coll;
                }
                if (def == typeof(Dictionary<,>))
                {
                    var d = (IDictionary)(existing ?? Activator.CreateInstance(t));
                    d.Clear();
                    foreach (var kv in n.Properties)
                    {
                        object key = args[0] == typeof(string) ? kv.Key
                            : args[0].IsEnum ? Enum.Parse(args[0], kv.Key)
                            : Convert.ChangeType(kv.Key, args[0], CultureInfo.InvariantCulture);
                        d[key] = Read(kv.Value, args[1], null);
                    }
                    return d;
                }
            }

            var obj = existing ?? Activator.CreateInstance(t);
            ReadInto(n, obj);
            return obj;
        }

        private static void ReadInto(JsonNode n, object target)
        {
            if (n.Kind != JsonKind.Object) return;
            foreach (var f in FieldsOf(target.GetType()))
            {
                var child = n.Get(f.Name);
                if (child == null) continue; // campo nuevo: conserva el valor por defecto (migraciones suaves)
                var current = f.GetValue(target);
                bool reuse = current != null && !f.FieldType.IsValueType && f.FieldType != typeof(string) && !f.FieldType.IsArray;
                var value = Read(child, f.FieldType, reuse ? current : null);
                if (!f.IsInitOnly || !reuse) f.SetValue(target, value);
            }
        }
    }
}
