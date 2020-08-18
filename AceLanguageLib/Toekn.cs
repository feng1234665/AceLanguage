using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AceLanguage
{
  public  struct Token
    {
      public  TokenType type;
        //所在行
        public int line;
       public string value;
    }
}
