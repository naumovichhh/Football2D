using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zevruk
{
    public sealed class Settings : IEnumerable<KeyValuePair<string, string>>
    {
        private const string Path = "settings.ini";
        private static SortedDictionary<string, string> defaultSettings = new SortedDictionary<string, string>
        {
            { "MatchDuration", "120" }, { "BallRebound", "1" }, { "ShouldAttack", "true" }
        };
        private SortedDictionary<string, string> settings = new SortedDictionary<string, string>();

        public string this[string str]
        {
            get
            {
                if (this.settings.ContainsKey(str))
                    return this.settings[str];
                else
                    return string.Empty;
            }

            set => this.settings[str] = value;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var e in this.settings)
                yield return e;
        }

        public void Read()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path))
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        var match = Regex.Match(str, @"(\w+)=(\w+)");
                        if (match.Groups.Count == 3)
                            this.settings[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
            }
            catch
            {
            }

            var missedSettings = defaultSettings.Where(s => !this.settings.ContainsKey(s.Key)).ToList();
            if (missedSettings.Count > 0)
            {
                foreach (var e in missedSettings)
                {
                    this.settings.Add(e.Key, e.Value);
                }
            }
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(Path))
            {
                foreach (var pair in this.settings)
                {
                    writer.WriteLine("{0}={1}", pair.Key, pair.Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
