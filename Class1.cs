using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Kinect.BodyStream
{
    class Card
    {
        public Boolean opened { 
            get; 
            set; }
        private String src = "";
        public Boolean matched { get; set; }
        public int id { get; set;  }
        public int match { get; set; }
        public string name { get; set; }

        public Image oculta { get; set; }
        public Image revelada { get; set; }
        public Card(int id, int match, string name, Image ocu, Image rev)
        {
            this.id = id;
            this.match = match;
            this.name = name;
            this.opened = false;
            this.oculta = ocu;
            this.revelada = rev;
        }

        public void turnCard()
        {
            this.opened = !this.opened;
            if (this.opened)
            {
                mostrar();
            }
            else
            {
                ocultar();
            }
        }

        public void ocultar()
        {
            revelada.Visibility = System.Windows.Visibility.Hidden;
            oculta.Visibility = System.Windows.Visibility.Visible;
            Console.WriteLine("entro a ocultar");
        }

        public void mostrar()
        {
            revelada.Visibility = System.Windows.Visibility.Visible;
            oculta.Visibility = System.Windows.Visibility.Hidden;
            Console.WriteLine("entro a mostrar");
        }
    }
}
