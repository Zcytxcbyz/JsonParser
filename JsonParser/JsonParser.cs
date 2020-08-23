using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonParser
{
    public enum JsonTokenType
    {
        START_OBJECT,   //  {
        END_OBJECT,     //  }
        START_ARRAY,    //  [
        END_ARRAY,      //  ]
        NULL,           //  null
        NUMBER,         //  number
        STRING,         //  string
        BOOLEAN,        //  boolean
        COLON,          //  ,
        COMMA,          //  :
        END_DOCUMENT    //  End Document
    }
    public class JsonToken
    {
        public JsonTokenType Type { get; set; }
        public String Value { get; set; }
        public JsonToken(String value,JsonTokenType type)
        {
            Value = value; Type = type;
        }
        public static JsonTokenList Tokenizer(string str)
        {
            str = str.Trim();
            JsonTokenList result = new JsonTokenList();
            int OBJ_SIGN = 0, ARR_SIGN = 0;
            for (int i = 0; i < str.Length; ++i) 
            {
                switch (str[i])
                {
                    case '{': result.Add(new JsonToken("{", JsonTokenType.START_OBJECT)); ++OBJ_SIGN; break;
                    case '}': result.Add(new JsonToken("}", JsonTokenType.END_OBJECT)); --OBJ_SIGN; break;
                    case '[': result.Add(new JsonToken("[", JsonTokenType.START_ARRAY)); ++ARR_SIGN; break;
                    case ']': result.Add(new JsonToken("]", JsonTokenType.END_ARRAY)); --ARR_SIGN; break;
                    case ',': 
                        result.Add(new JsonToken(",", JsonTokenType.COLON));
                        if (i == 0 || i == str.Length - 1 || str[i + 1] == '}' || str[i + 1] == ']') 
                            throw new FormatException("字符串格式不正确。");
                        break;
                    case ':': 
                        result.Add(new JsonToken(":", JsonTokenType.COMMA));
                        if (i == str.Length - 1 || str[i + 1] == '}' || str[i + 1] == ']' || str[i - 1] == '{' || str[i - 1] == '[') 
                        {
                            throw new FormatException("字符串格式不正确。");
                        }
                        break;
                    case '"':
                        {
                            String output = null;
                            bool IsEscape = false;
                            for (i += 1; true; ++i)
                            {
                                if(i >= str.Length)
                                    throw new FormatException("字符串格式不正确。");
                                if (str[i] == '\\')
                                {
                                    IsEscape = !IsEscape;
                                }
                                else if (str[i] == '"' && !IsEscape)
                                {
                                    break;
                                }
                                else
                                {
                                    IsEscape = false;
                                }
                                output += str[i];
                            }
                            result.Add(new JsonToken(output, JsonTokenType.STRING));
                        }
                        break;
                    case ' ': break;
                    default:
                        {
                            if (str[i] == '\n' || str[i] == '\r' || str[i] == '\t' || str[i] == '\v' || str[i] == '\f') break;
                            String output = null;
                            JsonTokenType type;
                            while (i < str.Length && str[i] != ',' && str[i] != '}' && str[i] != ']' && str[i] != ' ')                        
                            {
                                if(str[i] == '\n' || str[i] == '\r' || str[i] == '\t' || str[i] == '\v' || str[i] == '\f') break;
                                output += str[i];
                                ++i;
                            }
                            if (output == "true" || output == "false")
                            {
                                type = JsonTokenType.BOOLEAN;
                            }
                            else if (output == "null")
                            {
                                type = JsonTokenType.NULL;
                            }
                            else
                            {
                                type = JsonTokenType.NUMBER;
                                if (!double.TryParse(output, out _))
                                    throw new FormatException("字符串格式不正确。");
                            }
                            result.Add(new JsonToken(output, type));
                            if (i < str.Length) --i;
                        }
                        break;
                }
            }
            if (OBJ_SIGN != 0 || ARR_SIGN != 0) 
                throw new FormatException("字符串格式不正确。");
            result.Add(new JsonToken(null, JsonTokenType.END_DOCUMENT));
            return result;
        }
    }

    public class JsonTokenList : List<JsonToken>
    {
        public override string ToString()
        {
            string result = null;
            foreach(JsonToken token in this)
            {
                if (token.Type == JsonTokenType.STRING)
                {
                    result += '"' + token.Value + '"';
                }
                else
                {
                    result += token.Value;
                }       
            }
            return result;
        }
        private static void indent(ref string str, int depth)
        {
            for (int i = 0; i < depth; ++i)
                str += "    ";
        }
        public string Format()
        {
            string result = null;
            int depth = 0;
            for (int i = 0; i < this.Count; ++i) 
            {
                switch (this[i].Type)
                {
                    case JsonTokenType.START_ARRAY:
                        if (i > 1 && this[i - 1].Type == JsonTokenType.COMMA) 
                        {
                            result += '\n';
                            indent(ref result, depth);
                        }
                        ++depth;
                        result += "[ \n";
                        indent(ref result, depth);
                        break;
                    case JsonTokenType.END_ARRAY:
                        --depth;
                        result += '\n';
                        indent(ref result, depth);
                        result += "]";
                        break;
                    case JsonTokenType.START_OBJECT:
                        if (i > 1 && this[i - 1].Type == JsonTokenType.COMMA)
                        {
                            result += '\n';
                            indent(ref result, depth);
                        }
                        ++depth;
                        result += "{ \n";
                        indent(ref result, depth);
                        break;
                    case JsonTokenType.END_OBJECT:
                        --depth;
                        result += '\n';
                        indent(ref result, depth);
                        result += "}";
                        break;
                    case JsonTokenType.COLON:
                        result += " , \n";
                        indent(ref result, depth);
                        break;
                    case JsonTokenType.COMMA:
                        result += " : ";
                        break;
                    case JsonTokenType.STRING:
                        result += '"' + this[i].Value + '"';
                        break;
                    default:
                        result += this[i].Value;
                        break;
                }
            }
            return result;
        }
    }

    public class JsonObjectItem
    {
        public String Name { get; set; }
        public JsonValue Value { get; set; }
        public JsonObjectItem() { }
        public JsonObjectItem(String Name, JsonValue Value)
        {
            this.Name = Name;this.Value = Value;
        }
        public override string ToString()
        {
            return '"' + Name + "\":" + Value?.ToString();
        }
    }

    public class JsonObject : List<JsonObjectItem>
    {
        public override string ToString()
        {
            string result = "{";
            for (int i = 0; i < this.Count; ++i) 
            {
                result += this[i].ToString();
                if (i < this.Count - 1) result += ',';
            }
            result += "}";
            return result;
        }
        public JsonObjectItem this[string name]
        {
            get 
            { 
                return this.Find(t => t.Name == name); 
            }
            set 
            {
                this[FindIndex(t => t.Name == name)] = value;
            }
        }
        public static JsonObject Parser(String JsonStr)
        {
            JsonTokenList tokenList = JsonToken.Tokenizer(JsonStr);
            return JsonObject.Parser(tokenList);
        }
        public static JsonObject Parser(JsonTokenList tokenList)
        {
            bool mode = true;
            JsonObject result = new JsonObject();
            JsonObjectItem objectItem = new JsonObjectItem();
            for (int i = 1; i < tokenList.Count() - 1; ++i) 
            {
                switch (tokenList[i].Type)
                {
                    case JsonTokenType.COLON:
                        mode = true;
                        result.Add(objectItem);
                        objectItem = new JsonObjectItem();
                        break;
                    case JsonTokenType.COMMA:
                        mode = false;
                        break;
                    case JsonTokenType.STRING:
                        if (mode)
                        {
                            objectItem.Name = tokenList[i].Value;
                        }
                        else
                        {
                            objectItem.Value = new JsonValue(tokenList[i].Value, JsonValueType.STRING);
                        }
                        break;
                    case JsonTokenType.NUMBER:
                        objectItem.Value = new JsonValue(double.Parse(tokenList[i].Value), JsonValueType.NUMBER);
                        break;
                    case JsonTokenType.BOOLEAN:
                        objectItem.Value = new JsonValue(bool.Parse(tokenList[i].Value), JsonValueType.BOOLEAN);
                        break;
                    case JsonTokenType.NULL:
                        objectItem.Value = new JsonValue(null, JsonValueType.NULL);
                        break;
                    case JsonTokenType.START_OBJECT:
                        {
                            JsonTokenList JsonObj = new JsonTokenList();
                            while (i < tokenList.Count())
                            {
                                JsonObj.Add(tokenList[i]);
                                if (tokenList[i].Type == JsonTokenType.END_OBJECT)
                                    break;
                                ++i;
                            }
                            objectItem.Value = new JsonValue(JsonObject.Parser(JsonObj), JsonValueType.OBJECT);
                        }
                        break;
                    case JsonTokenType.START_ARRAY:
                        {
                            JsonTokenList JsonArr = new JsonTokenList();
                            while (i < tokenList.Count())
                            {
                                JsonArr.Add(tokenList[i]);
                                if (tokenList[i].Type == JsonTokenType.END_ARRAY)
                                    break;
                                ++i;
                            }
                            objectItem.Value = new JsonValue(JsonArray.Parser(JsonArr), JsonValueType.ARRAY);
                        }
                        break;
                }           
            }
            result.Add(objectItem);
            return result;
        }

        public static implicit operator JsonObject(JObject v)
        {
            if (v.Type == JsonValueType.OBJECT)
            {
                if (v.Data.GetType() == typeof(JsonObject))
                {
                    return v.Data as JsonObject;
                }
                else if(v.Data.GetType() == typeof(JsonObjectItem))
                {
                    return v.Data as JsonObjectItem;
                }
                
            }
            return null;
        }

        public static implicit operator JsonObject(JsonObjectItem v)
        {
            return new JsonObject { v };
        }
    }

    public class JsonArray : List<JsonValue>
    {
        public override string ToString()
        {
            string result = "[";
            for (int i = 0; i < this.Count; ++i)
            {
                result += this[i].ToString();
                if (i < this.Count - 1) result += ',';
            }
            result += "]";
            return result;
        }
        public JsonArray() { }

        public static JsonArray Parser(String JsonStr)
        {
            JsonTokenList tokenList = JsonToken.Tokenizer(JsonStr);
            return JsonArray.Parser(tokenList);
        }
        public static JsonArray Parser(JsonTokenList tokenList)
        {
            JsonArray result = new JsonArray();
            for (int i = 1; i < tokenList.Count() - 1; ++i) 
            {
                switch (tokenList[i].Type)
                {
                    case JsonTokenType.STRING:
                        result.Add(new JsonValue(tokenList[i].Value, JsonValueType.STRING));
                        break;
                    case JsonTokenType.NUMBER:
                        result.Add(new JsonValue(Double.Parse(tokenList[i].Value), JsonValueType.NUMBER));
                        break;
                    case JsonTokenType.NULL:
                        result.Add(new JsonValue(null, JsonValueType.NULL));
                        break;
                    case JsonTokenType.BOOLEAN:
                        result.Add(new JsonValue(bool.Parse(tokenList[i].Value), JsonValueType.NUMBER));
                        break;
                    case JsonTokenType.START_OBJECT:
                        {
                            JsonTokenList JsonObj = new JsonTokenList();
                            while (i < tokenList.Count())
                            {                           
                                JsonObj.Add(tokenList[i]);
                                if (tokenList[i].Type == JsonTokenType.END_OBJECT)
                                    break;
                                ++i;
                            }
                            result.Add(new JsonValue(JsonObject.Parser(JsonObj), JsonValueType.OBJECT));
                        }
                        break;
                    case JsonTokenType.START_ARRAY:
                        {
                            JsonTokenList JsonArr = new JsonTokenList();
                            while (i < tokenList.Count()) 
                            {
                                JsonArr.Add(tokenList[i]);
                                if (tokenList[i].Type == JsonTokenType.END_ARRAY)
                                    break;
                                ++i;
                            }
                            result.Add(new JsonValue(JsonArray.Parser(JsonArr), JsonValueType.ARRAY));
                        }
                        break;
                }
            }
            return result;
        }

        public static implicit operator JsonArray(JObject v)
        {
            if (v.Type == JsonValueType.ARRAY)
            {
                return v.Data as JsonArray;
            }
            return null;
        }
    }
    public enum JsonValueType
    {
        OBJECT, ARRAY, NULL, NUMBER, STRING, BOOLEAN, OBJECT_ITEM
    }
    public partial class JsonValue
    {
        public Object Value { get; set; }
        public JsonValueType Type { get; set; }
        public JsonValue this[int index]
        {
            get
            {
                if (Value.GetType() == typeof(JsonArray)) 
                    return ((JsonArray)Value)[index];
                else if (Value.GetType() == typeof(JsonObject))
                    return ((JsonObject)Value)[index];
                else
                    return null;
            }
            set
            {
                if (Value.GetType() == typeof(JsonArray))
                    ((JsonArray)Value)[index] = value;
                else if (Value.GetType() == typeof(JsonObject))
                    ((JsonObject)Value)[index] = value;
            }
        }
        public JsonValue this[string name]
        {
            get
            {
                if (Value.GetType() == typeof(JsonObject))
                    return ((JsonObject)Value)[name];
                else
                    return null;
            }
            set
            {
                if (Value.GetType() == typeof(JsonObject))
                    ((JsonObject)Value)[name] = value;
            }
        }
        public JsonValue() { }
        public JsonValue(Object value, JsonValueType type)
        {
            this.Value = value;this.Type = type;
        }
        public JsonValue(String value)
        {
            this.Value = value; this.Type = JsonValueType.STRING;
        }
        public JsonValue(Double value)
        {
            this.Value = value; this.Type = JsonValueType.NUMBER;
        }
        public JsonValue(bool value)
        {
            this.Value = value; this.Type = JsonValueType.BOOLEAN;
        }
        public JsonValue(JsonObject value)
        {
            this.Value = value; this.Type = JsonValueType.OBJECT;
        }
        public JsonValue(JsonArray value)
        {
            this.Value = value; this.Type = JsonValueType.ARRAY;

        }
        public override string ToString()
        {
            switch (Type)
            {
                case JsonValueType.STRING: return '"' + Value.ToString() + '"';
                case JsonValueType.NUMBER: return Value.ToString();
                case JsonValueType.ARRAY:return ((JsonArray)Value).ToString();
                case JsonValueType.OBJECT:
                    if (Value.GetType() == typeof(JsonObject))
                        return ((JsonObject)Value).ToString();
                    else if (Value.GetType() == typeof(JsonObjectItem))
                        return '{' + ((JsonObjectItem)Value).ToString() + '}';
                    else
                        return null;
                case JsonValueType.NULL: return "null";
                case JsonValueType.BOOLEAN: return (bool)Value ? "true" : "false";
                default: return null;
            }
        }
    }
    public partial class JsonValue
    {
        public static implicit operator JsonValue(String v)
        {
            return new JsonValue(v);
        }
        public static implicit operator JsonValue(Double v)
        {
            return new JsonValue(v);
        }
        public static implicit operator JsonValue(bool v)
        {
            return new JsonValue(v);
        }
        public static implicit operator JsonValue(JsonObject v)
        {
            return new JsonValue(v);
        }
        public static implicit operator JsonValue(JsonArray v)
        {
            return new JsonValue(v);
        }
        public static implicit operator String(JsonValue v)
        {
            return (String)v.Value;
        }
        public static implicit operator Double(JsonValue v)
        {
            return (Double)v.Value;
        }
        public static implicit operator bool(JsonValue v)
        {
            return (bool)v.Value;
        }
        public static implicit operator JsonObject(JsonValue v)
        {
            return (JsonObject)v.Value;
        }
        public static implicit operator JsonArray(JsonValue v)
        {
            return (JsonArray)v.Value;
        }
        public static implicit operator JObject(JsonValue v)
        {
            return new JObject(v.Value, v.Type);
        }
        public static implicit operator JsonValue(JObject v)
        {
            return new JsonValue(v.Data, v.Type);
        }
        public static implicit operator JsonValue(JsonObjectItem v)
        {
            return new JsonValue(v, JsonValueType.OBJECT_ITEM);
        }
        public static implicit operator JsonObjectItem(JsonValue v)
        {
            return (JsonObjectItem)v.Value;
        }
    }
    public class JObject
    {
        public Object Data { get; set; }
        public JsonValueType Type { get; set; }
        public override string ToString()
        {
            switch (Type)
            {
                case JsonValueType.STRING: return Data.ToString();
                case JsonValueType.NUMBER: return Data.ToString();
                case JsonValueType.ARRAY: return ((JsonArray)Data).ToString();
                case JsonValueType.OBJECT:
                    if (Data.GetType() == typeof(JsonObject))
                        return ((JsonObject)Data).ToString();
                    else if (Data.GetType() == typeof(JsonObjectItem))
                        return '{' + ((JsonObjectItem)Data).ToString() + '}';
                    else
                        return null;
                case JsonValueType.NULL: return "null";
                case JsonValueType.BOOLEAN: return (bool)Data ? "true" : "false";
                default: return null;
            }
        }
        public JObject() { }
        public JObject(Object Data, JsonValueType Type)
        {
            this.Data = Data;this.Type = Type;
        }
        public JsonValue this[int index]
        {
            get 
            {
                if (Data.GetType()==typeof(JsonArray))
                    return ((JsonArray)Data)[index];
                else if (Data.GetType()==typeof(JsonObject))
                    return ((JsonObject)Data)[index];
                else
                    return null;
            }
            set 
            {
                if (Data.GetType() == typeof(JsonArray))
                    ((JsonArray)Data)[index] = value;
                else if (Data.GetType() == typeof(JsonObject))
                    ((JsonObject)Data)[index] = value;
            }
        }
        public JsonValue this[string name]
        {
            get
            {
                if (Data.GetType() == typeof(JsonObject))
                    return ((JsonObject)Data)[name];
                else
                    return null;
            }
            set
            {
                if (Data.GetType() == typeof(JsonObject))
                    ((JsonObject)Data)[name] = value;
            }
        }
    }
    public class Json
    {
        public static string Compress(string JsonCode)
        {
            JsonTokenList jsonTokens = JsonToken.Tokenizer(JsonCode);
            return jsonTokens.ToString();
        }
        public static string Format(string JsonCode)
        {
            JsonTokenList jsonTokens = JsonToken.Tokenizer(JsonCode);
            return jsonTokens.Format();
        }
        public static JObject Parser(String JsonStr)
        {
            JsonTokenList tokenList = JsonToken.Tokenizer(JsonStr);
            return Json.Parser(tokenList);
        }
        public static JObject Parser(JsonTokenList tokenList)
        {
            JObject result = new JObject();
            switch (tokenList[0].Type)
            {
                case JsonTokenType.START_OBJECT:
                    {
                        result.Data = JsonObject.Parser(tokenList);
                        result.Type = JsonValueType.OBJECT;
                    }
                    break;
                case JsonTokenType.START_ARRAY:
                    {
                        result.Data = JsonArray.Parser(tokenList);
                        result.Type = JsonValueType.ARRAY;
                    }
                    break;
                case JsonTokenType.STRING:
                    {
                        result.Data = tokenList;
                        result.Type = JsonValueType.STRING;
                    }
                    break;
                case JsonTokenType.NUMBER:
                    {
                        result.Data = tokenList;
                        result.Type = JsonValueType.NUMBER;
                    }
                    break;
                case JsonTokenType.BOOLEAN:
                    {
                        result.Data = tokenList;
                        result.Type = JsonValueType.BOOLEAN;
                    }
                    break;
                case JsonTokenType.NULL:
                    {
                        result.Data = tokenList;
                        result.Type = JsonValueType.NULL;
                    }
                    break;
            }
            return result;
        }
    }
}
