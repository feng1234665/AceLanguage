using System;
using System.Collections.Generic;
using System.Text;

namespace AceLanguage
{
    /// <summary>
    /// 词法分析器
    /// </summary>
 public  class Lexical
    {
        public static List<Token> Process(string data)
        {
            line = 1;
            List<Token> list = new List<Token>();
            foreach (var item in data)
            {
                ReadOne(item, list);
            }
            if (lastValue.Length > 0)
            {
                AddNewToken(list);
            }
            return list;
        }
        static StringBuilder lastValue = new StringBuilder();
        static TokenType type = TokenType.NO_STATUS;
        static int line;
        static void ReadOne(char data, List<Token> list)
        {
            //如果当前处理的是字符串 直接往后添加数据
            if(type == TokenType.STRING_TOKEN)
            {
                if (data == '"')
                {
                    AddNewToken(list);
                }
                else
                {
                    lastValue.Append(data);
                }
                return;
            }
            //
            if (IsSpace(data))
            {
                //如果是空格 当前又没有已经读取的内容 直接丢弃掉这个空格
               if(type==TokenType.NO_STATUS)
                {
                    return;
                }
                AddNewToken(list);
                return;
            }

            if (IsSplit(data))
            {
                if (type != TokenType.NO_STATUS)
                {
                    AddNewToken(list);
                }
                lastValue.Append(data);
                type = SplitMapping(lastValue.ToString());
                AddNewToken(list);
                return;
            }

            
            if (IsOp(data))
            {
                if (type == TokenType.NO_STATUS|| IsOp())
                {
                    lastValue.Append(data);
                    type = OpMapping(lastValue.ToString());
                }
                else 
                {
                    AddNewToken(list);
                    lastValue.Append(data);
                    type = OpMapping(lastValue.ToString());
                }
                return;
            }
                    //判断一下前面的是否是运算符
            if (IsOp())
            {
                AddNewToken(list);
            }

            if (IsNum(data))
            {

                if (data == '.')
                {
                    if (type == TokenType.NO_STATUS)
                    {
                        type = TokenType.UNDEFINED_TOKEN;
                        lastValue.Append(data);
                        AddNewToken(list);
                        return;
                    }
                    if (type == TokenType.ERROR_TOKEN)
                    {
                        type = TokenType.ERROR_TOKEN;
                        lastValue.Append(data);
                        return;
                    }
                    if (type != TokenType.NUMBER_TOKEN)
                    {
                        AddNewToken(list);
                        type = TokenType.DOT_TOKEN;
                        lastValue.Append(data);
                        AddNewToken(list);
                        return;
                    }
                    if(type == TokenType.NUMBER_TOKEN)
                    {
                        if (lastValue.ToString().IndexOf('.')>-1) {
                            type = TokenType.ERROR_TOKEN;
                            lastValue.Append(data);
                            return;
                        }
                    }
                }

                if (type == TokenType.NO_STATUS)
                {
                    type = TokenType.NUMBER_TOKEN;
                }
                lastValue.Append(data);
                return;
            }
            if (data == '"') {
                if (type == TokenType.NO_STATUS) {
                    type = TokenType.STRING_TOKEN;
                    return;
                }
            }

            //以下为当前输入非数字 非分隔符 非空格情况
            if (type == TokenType.NUMBER_TOKEN)
            {
                type = TokenType.ERROR_TOKEN;
            }else if (type == TokenType.NO_STATUS)
            {
                type = TokenType.UNKNOWN_TOKEN;
            }
            lastValue.Append(data);

        }
        static bool IsNum(char data)
        {
            return (data >= '0' && data <= '9') || data == '.';
        }

        static bool IsOp()
        {
            return type>=TokenType.UNKNOWN_OP_TOKEN &&type<=TokenType.MXOR_TO_TOKEN;
        }

        //是否运算符
        static bool IsOp(char data) {
            return data == '+' || data == '-' || data == '*' || data == '/' || data == '%' || data == '>' || data == '<' || data == '=' || data == '&' || data == '|' || data == '^' || data == '~' || data == ':';
        }

        //是否空格符
        static bool IsSpace(char data)
        {
            if (data == '\n')
            {
                line++;
            }
            return data == ' '|| data == '\t'|| data == '\n' || data == '\r';
        }
        static bool IsSplit(char data)
        {
            return data == '(' || data == ')' || data == '[' || data == ']' || data == '{' || data == '}' || data == ',' || data == ';';
        }

        static void AddNewToken(List<Token> list)
        {

            if (type == TokenType.UNKNOWN_TOKEN)
            {
              type=  KeyWordMapping(lastValue.ToString());
            }

            list.Add(new Token() { type = type, value = lastValue.ToString(),line=line });
            lastValue.Clear();
            type = TokenType.NO_STATUS;
        }
        static TokenType SplitMapping(string data)
        {
            switch (data)
            {
                case "(":
                    return TokenType.BRACKETS_LEFT_TOKEN;
                case ")":
                    return TokenType.BRACKETS_RIGHT_TOKEN;
                case "[":
                    return TokenType.MIDDLE_BRACKETS_LEFT_TOKEN;
                case "]":
                    return TokenType.MIDDLE_BRACKETS_RIGHT_TOKEN;
                case "{":
                    return TokenType.BIG_BRACKETS_LEFT_TOKEN;
                case "}":
                    return TokenType.BIG_BRACKETS_RIGHT_TOKEN;
                case ",":
                    return TokenType.COMMA_TOKEN;
                case ";":
                    return TokenType.END_TOKEN;
                default:
                    return TokenType.UNKNOWN_TOKEN;
            }
        }

        static TokenType OpMapping(string data)
        {
            switch (data)
            {
                case "+":
                    return TokenType.ADD_TOKEN;
                case "-":
                    return TokenType.SUB_TOKEN;
                case "*":
                    return TokenType.MUL_TOKEN;
                case "/":
                    return TokenType.DIV_TOKEN;
                case "%":
                    return TokenType.MOD_TOKEN;
                case "&":
                    return TokenType.MAND_TOKEN;
                case "|":
                    return TokenType.MOR_TOKEN;
                case "^":
                    return TokenType.MXOR_TOKEN;
                case "~":
                    return TokenType.MNOT_TOKEN;
                case ">>":
                    return TokenType.SRIGHT_TOKEN;
                case "<<":
                    return TokenType.SLEFT_TOKEN;
                case ">":
                    return TokenType.MORE_TOKEN;
                case "<":
                    return TokenType.LESS_TOKEN;
                case "=":
                    return TokenType.SET_TOKEN;
                case ":":
                    return TokenType.COLON_TOKEN;
                case "<=":
                    return TokenType.LEQUAL_TOKEN;
                case ">=":
                    return TokenType.MEQUAL_TOKEN;
                case "==":
                    return TokenType.EQUAL_TOKEN;
                case "&&":
                    return TokenType.LAND_TOKEN;
                case "||":
                    return TokenType.LOR_TOKEN;
                case "!=":
                    return TokenType.NOT_EQUAL_TOKEN;
                case "!":
                    return TokenType.LNOT_TOKEN;
                case "+=":
                    return TokenType.ADD_TO_TOKEN;
                case "-=":
                    return TokenType.SUB_TO_TOKEN;
                case "*=":
                    return TokenType.MUL_TO_TOKEN;
                case "/=":
                    return TokenType.DIV_TO_TOKEN;
                case "%=":
                    return TokenType.MOD_TO_TOKEN;
                case ">>=":
                    return TokenType.SRIGHT_TO_TOKEN;
                case "<<=":
                    return TokenType.SLEFT_TO_TOKEN;
                case "&=":
                    return TokenType.MAND_TO_TOKEN;
                case "|=":
                    return TokenType.MOR_TO_TOKEN;
                case "^=":
                    return TokenType.MXOR_TO_TOKEN;
                default:
                    return TokenType.UNKNOWN_OP_TOKEN;
            }
        }

        static TokenType KeyWordMapping(string data)
        {
            switch (data)
            {
                case "int":
                case "float":
                case "long":
                case "short":
                case "bool":
                case "string":
                    return TokenType.IDENTIFIER_TOKEN;
                case "true":
                case "false":
                    return TokenType.BOOL_TOKEN;
                case "debug":
                    return TokenType.DEBUGGER_TOKEN;
                case "public":
                    return TokenType.PUBLIC_TOKEN;
                case "private":
                    return TokenType.PRIVATE_TOKEN;
                case "static":
                    return TokenType.STATIC_TOKEN;
                case "class":
                    return TokenType.CLASS_TOKEN;
                case "new":
                    return TokenType.NEW_TOKEN;
                case "operator":
                    return TokenType.OPERATOR_TOKEN;
                case "extends":
                    return TokenType.EXTENDS_TOKEN;
                case "void":
                    return TokenType.VOID_TOKEN;
                case "null":
                    return TokenType.NULL_TOKEN;
                case "undefined":
                    return TokenType.UNDEFINED_TOKEN;
                case "if":
                    return TokenType.IF_TOKEN;
                case "else":
                    return TokenType.ELSE_TOKEN;
                case "elif":
                    return TokenType.ELIF_TOKEN;
                case "switch":
                    return TokenType.SWITCH_TOKEN;
                case "case":
                    return TokenType.CASE_TOKEN;
                case "default":
                    return TokenType.DEFAULT_TOKEN;
                case "for":
                    return TokenType.FOR_TOKEN;
                case "while":
                    return TokenType.WHILE_TOKEN;
                case "continue":
                    return TokenType.CONTINUE_TOKEN;
                case "break":
                    return TokenType.BREAK_TOKEN;
                case "return":
                    return TokenType.RETURN_TOKEN;
                case "enum":
                    return TokenType.ENUM_TOKEN;
                case "using":
                    return TokenType.USING_TOKEN;
                case "import":
                    return TokenType.IMPORT_TOKEN;
                case "export":
                    return TokenType.EXPORT_TOKEN;
                case "as":
                    return TokenType.AS_TOKEN;
                case "delete":
                    return TokenType.DELETE_TOKEN;
                default:
                    return TokenType.UNKNOWN_TOKEN;
            }

        }
        static TokenType Mapping(string data)
        {
            switch (data)
            {
                case "+":
                    return TokenType.ADD_TOKEN;
                case "-":
                    return TokenType.SUB_TOKEN;
                case "*":
                    return TokenType.MUL_TOKEN;
                case "/":
                    return TokenType.DIV_TOKEN;
                case "%":
                    return TokenType.MOD_TOKEN;
                case ">":
                    return TokenType.MORE_TOKEN;
                case "<":
                    return TokenType.LESS_TOKEN;
                default:
                    return TokenType.IDENTIFIER_TOKEN;
            }
        }
    }
}
