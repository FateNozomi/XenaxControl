using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenaxControl
{
    public struct IntBits
    {
        private int bits;

        public IntBits(int initialBitValue)
        {
            this.bits = initialBitValue;
        }

        public bool this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    return (this.bits & (1 << index)) != 0;
                }
                else
                {
                    throw new IndexOutOfRangeException("Index cannot be less than zero.");
                }
            }

            set
            {
                // turn the bit on if value is true; otherwise, turn it off
                if (value)
                {
                    this.bits |= 1 << index;
                }
                else
                {
                    this.bits &= ~(1 << index);
                }
            }
        }
    }
}
