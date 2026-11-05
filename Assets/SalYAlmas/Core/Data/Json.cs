using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SalYAlmas.Core.Data
{
    public enum JsonKind { Null, Bool, Number, String, Array, Object }

    /// <summary>
    /// Nodo JSON mínimo. El núcleo no puede depender de UnityEngine (JsonUtility) ni de paquetes
    /// externos, así que trae su propio lector. Acepta comentarios // y /* */ para que los datos
    /// puedan marcar valores con // TUNING.
    /// </summary>
    public sealed class JsonNode
    {
        public JsonKind Kind { get; private set; }
        public string Path { get; private set; }
        public int Line { get; private set; }

        private bool _bool;
        private double _number;
        private string _string;
        private List<JsonNode> _items;
        private List<KeyValuePair<string, JsonNode>> _props;

        private JsonNode(JsonKind kind, string path, int line)
        {
            Kind = kind;
            Path = path;
            Line = line;
        }

        public bool AsBool => _bool;
        public double AsNumber => _number;
        public string AsString => _string;
        public IReadOnlyList<JsonNode> Items => _items ?? (IReadOnlyList<JsonNode>)Array.Empty<JsonNode>();
        public IReadOnlyList<KeyValuePair<string, JsonNode>> Properties =>
            _props ?? (IReadOnlyList<KeyValuePair<string, JsonNode>>)Array.Empty<KeyValuePair<string, JsonNode>>();

        public bool Has(string key) => Get(key) != null;

        public JsonNode Get(string key)
        {
            if (_props == null) return null;
            for (int i = 0; i < _props.Count; i++)
                if (_props[i].Key == key) return _props[i].Value;
            return null;
        }

        internal static JsonNode MakeNull(string path, int line) => new JsonNode(JsonKind.Null, path, line);
        internal static JsonNode MakeBool(bool v, string path, int line) => new JsonNode(JsonKind.Bool, path, line) { _bool = v };
        internal static JsonNode MakeNumber(double v, string path, int line) => new JsonNode(JsonKind.Number, path, line) { _number = v };
        internal static JsonNode MakeString(string v, string path, int line) => new JsonNode(JsonKind.String, path, line) { _string = v };
        internal static JsonNode MakeArray(string path, int line) => new JsonNode(JsonKind.Array, path, line) { _items = new List<JsonNode>() };
        internal static JsonNode MakeObject(string path, int line) => new JsonNode(JsonKind.Object, path, line) { _props = new List<KeyValuePair<string, JsonNode>>() };

        internal void AddItem(JsonNode n) => _items.Add(n);
        internal void AddProp(string k, JsonNode n) => _props.Add(new KeyValuePair<string, JsonNode>(k, n));
    }

    public sealed class JsonParseException : Exception
    {
        public JsonParseException(string message) : base(message) { }
    }

    public static class Json
    {
        public static JsonNode Parse(string text, string sourceName = "json")
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var p = new Parser(text, sourceName);
            p.SkipWs();
            var root = p.ParseValue("$");
            p.SkipWs();
            if (!p.AtEnd) throw p.Error("contenido extra después del valor raíz");
            return root;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private readonly string _source;
            private int _i;
            private int _line = 1;

            public Parser(string s, string source)
            {
                _s = s;
                _source = source;
                // Ignora el BOM si el archivo lo trae.
                if (_s.Length > 0 && _s[0] == '﻿') _i = 1;
            }

            public bool AtEnd => _i >= _s.Length;

            public JsonParseException Error(string msg) =>
                new JsonParseException($"{_source}:{_line}: {msg}");

            public void SkipWs()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == '\n') { _line++; _i++; }
                    else if (c == ' ' || c == '\t' || c == '\r') _i++;
                    else if (c == '/' && _i + 1 < _s.Length && _s[_i + 1] == '/')
                    {
                        while (_i < _s.Length && _s[_i] != '\n') _i++;
                    }
                    else if (c == '/' && _i + 1 < _s.Length && _s[_i + 1] == '*')
                    {
                        _i += 2;
                        while (_i + 1 < _s.Length && !(_s[_i] == '*' && _s[_i + 1] == '/'))
                        {
                            if (_s[_i] == '\n') _line++;
                            _i++;
                        }
                        if (_i + 1 >= _s.Length) throw Error("comentario /* sin cerrar");
                        _i += 2;
                    }
                    else break;
                }
            }

            public JsonNode ParseValue(string path)
            {
                SkipWs();
                if (AtEnd) throw Error("se esperaba un valor");
                char c = _s[_i];
                int line = _line;
                switch (c)
                {
                    case '{': return ParseObject(path);
                    case '[': return ParseArray(path);
                    case '"': return JsonNode.MakeString(ParseString(), path, line);
                    case 't': Expect("true"); return JsonNode.MakeBool(true, path, line);
                    case 'f': Expect("false"); return JsonNode.MakeBool(false, path, line);
                    case 'n': Expect("null"); return JsonNode.MakeNull(path, line);
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return JsonNode.MakeNumber(ParseNumber(), path, line);
                        throw Error($"carácter inesperado '{c}'");
                }
            }

            private void Expect(string word)
            {
                if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0) throw Error($"se esperaba '{word}'");
                _i += word.Length;
            }

            private JsonNode ParseObject(string path)
            {
                var node = JsonNode.MakeObject(path, _line);
                _i++; // {
                SkipWs();
                if (!AtEnd && _s[_i] == '}') { _i++; return node; }
                while (true)
                {
                    SkipWs();
                    if (AtEnd || _s[_i] != '"') throw Error("se esperaba el nombre de una propiedad entre comillas");
                    string key = ParseString();
                    if (node.Get(key) != null) throw Error($"propiedad duplicada '{key}'");
                    SkipWs();
                    if (AtEnd || _s[_i] != ':') throw Error("se esperaba ':'");
                    _i++;
                    var value = ParseValue(path + "." + key);
                    node.AddProp(key, value);
                    SkipWs();
                    if (AtEnd) throw Error("objeto sin cerrar");
                    if (_s[_i] == ',')
                    {
                        _i++;
                        SkipWs();
                        // Tolera una coma final antes de '}'.
                        if (!AtEnd && _s[_i] == '}') { _i++; return node; }
                        continue;
                    }
                    if (_s[_i] == '}') { _i++; return node; }
                    throw Error("se esperaba ',' o '}'");
                }
            }

            private JsonNode ParseArray(string path)
            {
                var node = JsonNode.MakeArray(path, _line);
                _i++; // [
                SkipWs();
                if (!AtEnd && _s[_i] == ']') { _i++; return node; }
                int index = 0;
                while (true)
                {
                    node.AddItem(ParseValue(path + "[" + index + "]"));
                    index++;
                    SkipWs();
                    if (AtEnd) throw Error("arreglo sin cerrar");
                    if (_s[_i] == ',')
                    {
                        _i++;
                        SkipWs();
                        if (!AtEnd && _s[_i] == ']') { _i++; return node; }
                        continue;
                    }
                    if (_s[_i] == ']') { _i++; return node; }
                    throw Error("se esperaba ',' o ']'");
                }
            }

            private string ParseString()
            {
                _i++; // "
                var sb = new StringBuilder();
                while (true)
                {
                    if (AtEnd) throw Error("cadena sin cerrar");
                    char c = _s[_i++];
                    if (c == '"') break;
                    if (c == '\n') throw Error("salto de línea dentro de una cadena");
                    if (c != '\\') { sb.Append(c); continue; }
                    if (AtEnd) throw Error("escape incompleto");
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw Error("escape \\u incompleto");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default: throw Error($"escape desconocido '\\{e}'");
                    }
                }
                return sb.ToString();
            }

            private double ParseNumber()
            {
                int start = _i;
                if (_s[_i] == '-') _i++;
                while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.' || _s[_i] == 'e' || _s[_i] == 'E' || _s[_i] == '+' || _s[_i] == '-'))
                    _i++;
                string token = _s.Substring(start, _i - start);
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    throw Error($"número inválido '{token}'");
                return v;
            }
        }
    }
}
