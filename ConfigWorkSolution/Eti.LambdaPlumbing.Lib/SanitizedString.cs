using System;
using System.Collections.Generic;
using System.Web;
using Ganss.XSS;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Eti.LambdaPlumbing
{
    [JsonConverter(typeof(SanitizedStringConverter))]
    public class SanitizedString
    {
        private static readonly Lazy<HtmlSanitizer> _defaultSanitizerLazy = new Lazy<HtmlSanitizer>(() =>
        {
            var san = new HtmlSanitizer();
            san.AllowedTags.Clear();
            san.UriAttributes.Clear();
            san.AllowedAttributes.Clear();
            san.AllowedCssProperties.Clear();
            san.AllowedClasses.Clear();
            san.AllowedSchemes.Clear();
            san.AllowedAtRules.Clear();
            return san;
        });

        private readonly string _original;
        private readonly string _sanitized;
        
        public string Original
        {
            get { return _original; }
        }

        public string Sanitized
        {
            get { return _sanitized; }
        }

        /// <summary>
        /// Constructor to use when you want to tweak the sanitization rules.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="allowedTags"></param>
        /// <param name="allowedSchemes"></param>
        /// <param name="allowedAttributes"></param>
        /// <param name="uriAttributes"></param>
        /// <param name="allowedCssProperties"></param>
        /// <remarks>see https://github.com/mganss/HtmlSanitizer for rule info</remarks>
        public SanitizedString(string s, IEnumerable<string> allowedTags = null,
            IEnumerable<string> allowedSchemes = null,
            IEnumerable<string> allowedAttributes = null, 
            IEnumerable<string> uriAttributes = null,
            IEnumerable<string> allowedCssProperties = null)
            : this(new HtmlSanitizer(allowedTags, allowedSchemes, allowedAttributes, uriAttributes, allowedCssProperties), s)
        {
            // this one creates a custom HtmlSanitizer
        }
        
        /// <summary>
        /// Normal constructor, used to strip all XSSable content out of the string
        /// </summary>
        /// <param name="s"></param>
        public SanitizedString(string s) : this(_defaultSanitizerLazy.Value, s)
        {
            //note that this one uses our static HtmlSanitizer instance, because this is the normal use case
            //to take care of Json serialization:
            //https://stackoverflow.com/questions/24472404/json-net-how-to-serialize-object-as-value
        }

        private SanitizedString(HtmlSanitizer sanitizer, string s)
        {
            _original = s;
            _sanitized = HttpUtility.HtmlDecode(sanitizer.Sanitize(s));
        }
        
        public override string ToString()
        {
            return _sanitized;
        }

        public override int GetHashCode()
        {
            return _sanitized.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is string objString)
            {
                return _sanitized.Equals(objString);
            }
            else if (obj is SanitizedString objSan)
            {
                return _sanitized.Equals(objSan.ToString());
            }
            else
            {
                return false;
            }
        }
        public static bool operator ==(SanitizedString lhs, SanitizedString rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    return true;
                }
                return false;
            }
            
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SanitizedString lhs, SanitizedString rhs)
        {
            return !(lhs == rhs);
        }
        
        public static implicit operator string(SanitizedString ss) => ss.Sanitized;
        public static explicit operator SanitizedString(string s) => new SanitizedString(s);

    }
    
    #region JsonConverter implementation

    /// <summary>
    /// Note, this Converter only works for Newtonsoft.Json- if using System.Text.Json,
    /// and using the JsonConverterAttribute, it will just be ignored. Thanks, Microsoft.
    /// </summary>
    public class SanitizedStringConverter : JsonConverter<SanitizedString>
    {
        public override void WriteJson(JsonWriter writer, SanitizedString value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override SanitizedString ReadJson(JsonReader reader, Type objectType, SanitizedString existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            string json = (string) reader.Value;
            return new SanitizedString(json);
        }
        
       
    }
    #endregion
    
    
}