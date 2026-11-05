#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Captura la salida real del listener, sin modificarla. Solo existe en el Editor.</summary>
    [RequireComponent(typeof(AudioListener))]
    public sealed class AudioMixRecorder : MonoBehaviour
    {
        private float[] _buffer;
        private int _written, _sampleRate, _channels = 2;
        private volatile bool _recording;
        private string _path;

        public void Begin(string path, int seconds = 15)
        {
            if (AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
                throw new InvalidOperationException("La captura de revisión requiere salida estéreo.");
            _sampleRate = AudioSettings.outputSampleRate;
            _path = Path.GetFullPath(path);
            _buffer = new float[_sampleRate * 2 * Mathf.Clamp(seconds, 1, 60)];
            _written = 0;
            _recording = true;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_recording || _buffer == null) return;
            _channels = channels;
            int count = Math.Min(data.Length, _buffer.Length - _written);
            Array.Copy(data, 0, _buffer, _written, count);
            int written = _written + count;
            Volatile.Write(ref _written, written);
            if (written >= _buffer.Length) _recording = false;
        }

        private void Update()
        {
            if (_buffer != null && !_recording) Finish();
        }

        public void Finish()
        {
            _recording = false;
            if (_buffer == null) return;
            int count = Volatile.Read(ref _written);
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            float peak = 0f;
            int clipped = 0;
            using (var writer = new BinaryWriter(File.Create(_path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1);
                writer.Write((short)_channels); writer.Write(_sampleRate); writer.Write(_sampleRate * _channels * 2);
                writer.Write((short)(_channels * 2)); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                for (int i = 0; i < count; i++)
                {
                    float sample = _buffer[i];
                    peak = Mathf.Max(peak, Mathf.Abs(sample));
                    if (Mathf.Abs(sample) >= 1f) clipped++;
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 32767f / 32768f) * 32768f));
                }
            }
            Debug.Log($"Revisión de mezcla: {_path} · {count / (float)(_sampleRate * _channels):F1} s · pico {20f * Mathf.Log10(Mathf.Max(peak, 1e-9f)):F2} dBFS · {clipped} muestras saturadas.");
            _buffer = null;
            Destroy(this);
        }

        [MenuItem("Sal y Almas/Audio/Grabar salida (15 s)")]
        private static void Record()
        {
            if (!Application.isPlaying) { Debug.LogWarning("Entra en Play para grabar la mezcla."); return; }
            var listener = FindAnyObjectByType<AudioListener>();
            if (listener == null || listener.GetComponent<AudioMixRecorder>() != null) return;
            listener.gameObject.AddComponent<AudioMixRecorder>().Begin("Logs/audio-review.wav");
        }
    }
}

#endif
