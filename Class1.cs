using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.BodyStream
{
    class Card
    {
        public Boolean opened { get; set; }
        private String src = "";
        public int id { get; set;  }
        public int match { get; set; }
        public string name { get; set; }

        public Card(int id, int match, string name)
        {
            this.id = id;
            this.match = match;
            this.name = name;
            this.opened = false;
        }

        public void turnCard()
        {
            this.opened = !this.opened;
        }
    }
}
