using Eti.LambdaPlumbing;
using Newtonsoft.Json;
using Xunit;

namespace Test.LambdaPlumbing
{
    public class SanitizedStringTest
    {
        [Fact]
        public void ExplicitStringCast()
        {
            string foo = "foo";
            SanitizedString ssTest = (SanitizedString) foo;
            Assert.Equal(foo, ssTest.Sanitized);

            SanitizedString ssTest2 = (SanitizedString) "bar";
            Assert.Equal("bar", ssTest2.Sanitized);
        }

        [Fact]
        public void ImplicitStringCast()
        {
            SanitizedString ssTest = new SanitizedString("monkey");
            string s = ssTest;
            Assert.Equal(ssTest.Sanitized, s);

            
            
            string bad = "string @with some <badthings/>";
            SanitizedString ssTest2 = new SanitizedString(bad);
            Assert.NotEqual(bad, ssTest2);
            
        }

        [Fact]
        public void ValueCastEquality()
        {
            //yeah, a little crazy...
            SanitizedString ss = new SanitizedString("foo");
            Assert.Equal("foo", ss);
        }
        
        [Fact]
        public void ValueEquality()
        {
        
            SanitizedString ss1 = new SanitizedString("monkey");
            SanitizedString ss2 = new SanitizedString("monkey");
            Assert.Equal(ss1, ss2);
        }

        [Fact]
        public void ValueNotEqual()
        {
            SanitizedString ss1 = new SanitizedString("monkey");
            SanitizedString ss2 = new SanitizedString("pangolin");
            Assert.NotEqual(ss1, ss2);
        }

        [Fact]
        public void OriginalValueCheck()
        {
            string badMonkey = "mo<script>nkey";
            SanitizedString ss1 = new SanitizedString(badMonkey);
            Assert.NotEqual(badMonkey, ss1.ToString());
            Assert.Equal(badMonkey, ss1.Original);
            
        }

        [Fact]
        public void StringConcatenation()
        {
            SanitizedString ss1 = new SanitizedString("monkey");
            string test = "good" + ss1;
            
            Assert.Equal("goodmonkey", test);

            string test2 = "foo<script>bar";
            string result = ss1 + test2;
            Assert.Equal("monkeyfoo<script>bar", result);
            
        }

        [Fact]
        public void SanitizedStringConcatenation()
        {
            SanitizedString ss1 = new SanitizedString("monkey");
            SanitizedString ss2 = new SanitizedString("chow<script>awk");
            string s = ss1 + ss2;
            Assert.Equal("monkeychow", s);
        }
        
        [Fact]
        public void JsonSerializeAsString()
        {
            //the JsonConverter serializes SanitizedString as a regular string
            //note, have to make sure we're using Newtonsoft.Json, not the MS version
            SanitizedString ssTest = new SanitizedString("foo");

            string json = JsonConvert.SerializeObject(ssTest);
            string expected = "\"foo\"";
            Assert.Equal(expected, json);

        }

        [Fact]
        public void SimpleJsonDeserialization()
        {
            //the JsonConverter serializes SanitizedString as a regular string
            //note, have to make sure we're using Newtonsoft.Json, not the MS version
            string goodJson = "\"monkey\"";
            SanitizedString ss1 = JsonConvert.DeserializeObject<SanitizedString>(goodJson);
            Assert.Equal("monkey", ss1.Sanitized);

           
        }

        [Fact]
        public void JsonDeserializationWithScriptTag()
        {
            string jsonWithBadness = "\"monkey<script>with badness\"";
            SanitizedString ss2 = JsonConvert.DeserializeObject<SanitizedString>(jsonWithBadness);
            Assert.Equal("monkey", ss2.Sanitized);
        }
        
        [Fact]
        public void BadJsonDeserializationShouldThrow()
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                string badJson = @"{ 'somekey':'someval'}";
                SanitizedString ss3 = JsonConvert.DeserializeObject<SanitizedString>(badJson);

            });
        }

        //TODO if i discover any places that are using custom sanitation rules, i'll write some tests for them
        
        [Theory]
        [InlineData("monkey", "monkey")]
        [InlineData("mon<script>key", "mon")]
        [InlineData("&monkey", "&monkey")]
        [InlineData("@monkey", "@monkey")]
        [InlineData("https://monkey", "https://monkey")]
        [InlineData("https://monkey.com/foo", "https://monkey.com/foo")]
        [InlineData("https://monkey.com/foo<style>", "https://monkey.com/foo")]
        public void SanitizerScenarios(string inputVal, string expected)
        {
            //note, i'm not doing in-depth testing of the actual sanitization functionality,
            //because it's been done repeatedly elsewhere.
            SanitizedString ssTest = new SanitizedString(inputVal);
            Assert.Equal(expected, ssTest.Sanitized);
        }
        
        
    }
}