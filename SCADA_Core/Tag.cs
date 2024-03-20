using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class Tag
    {
        public string TagName { get; set; }

        public string Description { get; set; }

        public string IOAddress { get; set; }

        public Tag() { }

        public Tag(string tag, string desc, string io)
        {
            TagName = tag;
            Description = desc;
            IOAddress = io;
        }
    }

    public class TagValue
    {
        [Key]
        public int Id { get; set; }
        public string TagName { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public DateTime Time { get; set; }

        public override string ToString() 
        {
            return $"Tag name: {TagName}, Tag: {Tag}, Type: {Type}, Value: {Value}, Time: {Time}";
        }
    }
}