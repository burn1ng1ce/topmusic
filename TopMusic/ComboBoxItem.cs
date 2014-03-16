using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    class ComboBoxItem
    {
        private string _text;
        private object _value;

        public string Text {
            get { return this._text; }
            set { this._text = value;  }
        }
        public object Value {
            get { return this._value; }
            set { this._value = value; }
        }

        public ComboBoxItem(String text, object value)
        {
            this.Text = text;
            this.Value = value;
        }

        public override string ToString()
        {
            return this._text;
        }
    }
}
