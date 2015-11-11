using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhtReadService
{
    public sealed class Dht11Reading
    {
        ulong value;
        public Dht11Reading(ulong uvalue)
        {
            this.value = uvalue;
        }
        public bool IsValid
        {
            get
            {
                ulong checksum =
                    ((value >> 32) & 0xff) +
                    ((value >> 24) & 0xff) +
                    ((value >> 16) & 0xff) +
                    ((value >> 8) & 0xff);

                return (checksum & 0xff) == (value & 0xff);
            }
        }

        public double Humidity
        {
            get
            {
                return (((value >> 24) & 0xff00) + ((value >> 24) & 0xff)) / 10.0;
            }
        }

        public double Temperature
        {
            get
            {
                double f = (((value >> 8) & 0x7f00) + ((value >> 8) & 0xff)) / 10.0;
                if (((value >> 16) & 0x80) > 0) f *= -1;
                return f;
            }
        }
    }
}
