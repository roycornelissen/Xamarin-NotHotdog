using System.Collections.Generic;

namespace NotHotdog.Model
{
    public class Category
    {
        public string name { get; set; }
        public double score { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public double confidence { get; set; }
    }

    public class Description
    {
        public List<string> tags { get; set; }
        public List<Caption> captions { get; set; }
    }

    public class Metadata
    {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

    public class CognitiveResults
    {
        public List<Category> categories { get; set; }
        public List<Tag> tags { get; set; }
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
    }
}