using Newtonsoft.Json;

namespace Aerolt_External
{
    public class WebsocketMessage
    {
        private string _type;
        public WebsocketMessage(string serializedData) => JsonConvert.PopulateObject(serializedData, this);

        public WebsocketMessage() {}

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string type
        {
            get { return _type ??= this.GetType().Name; }
            set
            {
                _type = value;
                if (_type != this.GetType().Name && _type != nameof(WebsocketMessage)) Console.WriteLine("[Warn] WebsocketMessage deserialized with the wrong type(" + _type + "). Expecting: " + this.GetType().Name);
            }
        }
    }
}