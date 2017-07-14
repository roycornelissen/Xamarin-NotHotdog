using System;
using System.Collections.Generic;
using MvvmHelpers;

namespace NotHotdog.Model
{
    public class RecognizedHotdog : ObservableObject
    {
        public RecognizedHotdog()
        {
        }

		bool hotdog;
		public bool Hotdog
		{
			get { return hotdog; }
			set { SetProperty(ref hotdog, value); }
		}

        public double Certainty { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }

    }
}
