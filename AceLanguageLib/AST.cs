using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AceLanguage
{
    /// <summary>
    /// 语法分析器
    /// </summary>
   public class AST
    {
        
        Token[] tokens;
        int index = 0;
        private AST(Token[] tokens)
        {
            this.tokens = tokens;
        }

        void Next()
        {
            index++;
        }
        void Jump(int step)
        {
            index += step;
        }
        void Before()
        {
            index--;
        }
        
        public ASTNode Prase()
        {
            BlockNode node = new BlockNode() { type = NodeType.BLOCK };

            while (index<tokens.Length-1)
            {
                var token = tokens[index];
                ASTNode temp;
                switch (token.type)
                {
                    case TokenType.ENUM_TOKEN:
                        Next();
                        temp=ReadDefEnum(TokenType.PRIVATE_TOKEN,  0);
                        if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp);
                        break;
                    case TokenType.USING_TOKEN:
                        Next();
                         temp=ReadUsing();
                        if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp);
                        break;
                    case TokenType.IMPORT_TOKEN:
                        break;
                    case TokenType.CLASS_TOKEN:
                        Next();
                        temp=ReadClass(TokenType.PRIVATE_TOKEN, false,   0);
                        if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp);
                        break;
                    case TokenType.PUBLIC_TOKEN:
                    case TokenType.PRIVATE_TOKEN:
                        
                        if (tokens[index + 1].type == TokenType.STATIC_TOKEN)
                        {
                            Jump(2);
                            temp=ReadAuthority(token.type,true,  0);
                        }
                        else
                        {
                            Next();
                            temp=ReadAuthority(token.type, false, 0);
                        }
                        if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp);
                        break;
                    case TokenType.STATIC_TOKEN:
                        Next();
                        token = tokens[index];
                        if (token.type == TokenType.PUBLIC_TOKEN || token.type == TokenType.PRIVATE_TOKEN)
                        {
                            Next();
                            temp=ReadAuthority(token.type, true, 0);
                        }
                        else
                        {
                            temp=ReadAuthority(token.type, false, 0);
                        }
                        if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp);
                        break;
                    case TokenType.NOTE_TOKEN:
                        break;
                    default:
                        return new ErrorNode() { type = NodeType.ERROR, error = "未知定义" + token.value + "在第" + token.line + "行" };
                }
            }
            return node;
        }

        ASTNode ReadUsing()
        {
            Token token = tokens[index];
            if(token.type==TokenType.STRING_TOKEN && tokens[index + 1].type == TokenType.END_TOKEN)
            {
                Jump(2);
                return new UsingNode() { path=token.value};
            }
            else
            {
                return new ErrorNode() { error = "语法错误 引入文件不存在" };
            }
        }
        public static ASTNode Process(Token[] tokens) {
            return new AST(tokens).Prase();
        }
        //读取到权限关键字  只能在第0层和第一层 即定义class及class成员变量或者方法
        ASTNode ReadAuthority(TokenType authority,bool isstatic, int depth)
        {

            if (depth >1) { return new ErrorNode() {error="语法错误 在第"+tokens[index-1].line+"行，"+ tokens[index - 1].value }; };
            Token token = tokens[index];
            switch (token.type)
            {
                case TokenType.UNKNOWN_TOKEN:
                case TokenType.IDENTIFIER_TOKEN:
                    if (depth == 0)
                    {
                        return new ErrorNode() { error = "语法错误 不能在class外定义内容 在第" + token.line + "行，" + token.value };
                    }
                    else
                    {
                        string valueType = ReadDefVarType();
                        token = tokens[index];
                        if (token.type != TokenType.UNKNOWN_TOKEN)
                        {
                            return new ErrorNode() { error = "语法错误 在第" + token.line + "行，" + token.value };
                        }
                        string varName = token.value;
                        Next();
                        token= tokens[index];

                        if (token.type == TokenType.BRACKETS_LEFT_TOKEN)
                        {
                           return ReadDefFunc(valueType, varName, authority, isstatic,  depth);
                        }
                        else
                        {
                          return  ReadDefClassVar(valueType, varName, authority, isstatic, depth);
                        }
                    }          
                case TokenType.ENUM_TOKEN:
                    if(isstatic)return new ErrorNode() { error = "语法错误 static不适用与enum 在第" + token.line + "行，" + token.value };
                    Next();
                    return ReadDefEnum( authority,  depth);           
                case TokenType.CLASS_TOKEN:
                    if (depth > 0) return new ErrorNode() { error = "语法错误 在第" + token.line + "行，" + token.value };
                    Next();
                    return ReadClass( authority, isstatic,  depth);           
                case TokenType.OPERATOR_TOKEN:
                    if (depth == 0)
                    {
                        return new ErrorNode() { error = "语法错误 不能在class外定义内容 在第" + token.line + "行，" + token.value };
                    }
                    else
                    {
                        //TODO 运算符重载
                        return null;
                    }
                case TokenType.VOID_TOKEN:
                    if (depth == 0)
                    {
                        return new ErrorNode() { error = "语法错误 不能在class外定义内容 在第" + token.line + "行，" + token.value };
                    }
                    else
                    {
                        Next();
                        token = tokens[index];
                        if (token.type != TokenType.UNKNOWN_TOKEN)
                        {
                               return new ErrorNode() { error = "语法错误 在第" + token.line + "行，" + token.value };
                        }
                        if (tokens[index+1].type == TokenType.BRACKETS_LEFT_TOKEN)
                        {
                            Next();
                          return  ReadDefFunc("void", token.value, authority, isstatic,  depth);
                        }
                        else
                        {
                          return new ErrorNode() { error = "语法错误 无法定义一个void类型的变量 在第" + token.line + "行，" + token.value };
                        }
                    }
                default:
                    return new ErrorNode() { error = "语法错误 在第" + token.line + "行，" + token.value };
            }
        }
        //申明变量的情况下 如果后面跟的是一组中括号 就是数组
        string ReadDefVarType()
        {
            Token token = tokens[index];
            string type = token.value;
            Next();
            while (tokens[index].type == TokenType.MIDDLE_BRACKETS_LEFT_TOKEN && tokens[index + 1].type == TokenType.MIDDLE_BRACKETS_RIGHT_TOKEN)
            {
                type += "[]";
                Jump(2);
            }
            return type;
        }

        ASTNode ReadClass(TokenType authority, bool isstatic,  int depth)
        {
            Token token = tokens[index];
            if (token.type != TokenType.UNKNOWN_TOKEN)
            {
                return new ErrorNode() { error = "类定义语法错误 在第" + token.line + "行，" + token.value };
            }
            string className = token.value;
            Next();
            token = tokens[index];
            if (token.type != TokenType.BIG_BRACKETS_LEFT_TOKEN)
            {
                return new ErrorNode() { error = "类定义语法错误 在第" + token.line + "行，" + token.value };
            }
            ASTNode body= ReadBlock(depth + 1);
            if (body.type != NodeType.ERROR)
            {
               return new ClassNode() { AUTHORITY = authority, isstatic = isstatic, body = body,className=className };
            }
            return body;
        }
        ASTNode ReadDefClassVar(string varType,string varName, TokenType authority, bool isstatic, int depth)
        {
            Token token= tokens[index];
            if (token.type == TokenType.END_TOKEN)
            {
                Next();
                return new ClassVarNode() { authority = authority,isstatic=isstatic,varName=varName,varType=varType};
            }
            if (token.type == TokenType.SET_TOKEN) {
                Next();
                token = tokens[index];
                if (token.type == TokenType.NEW_TOKEN)
                {
                    Next();
                    var rn=ReadNew();
                    if (rn.type == NodeType.ERROR) return rn;
                    return new ClassVarNode() { authority = authority, isstatic = isstatic, varName = varName, varType = varType, body = rn };
                }
                else if(token.type==TokenType.UNKNOWN_TOKEN || token.type==TokenType.NUMBER_TOKEN|| token.type == TokenType.STRING_TOKEN|| token.type == TokenType.BOOL_TOKEN)
                {
                    if (tokens[index+1].type == TokenType.END_TOKEN)
                    {
                        Jump(2);
                        return new ClassVarNode() { authority = authority, isstatic = isstatic, varName = varName, varType = varType,body=new ValueNode() { valueType=token.type,value=token.value} };
                    }
                    if(tokens[index+1].type==TokenType.BRACKETS_LEFT_TOKEN ||(tokens[index + 1].type>=TokenType.ADD_TOKEN&& tokens[index + 1].type<=TokenType.MOD_TOKEN))
                    {
                        ASTNode node= ReadExpression(TokenType.END_TOKEN);
                        if (node.type == NodeType.ERROR) return node;
                        return new ClassVarNode() { authority = authority, isstatic = isstatic, varName = varName, varType = varType, body = node };
                    }
                }else
                {
                    ASTNode node = ReadExpression(TokenType.END_TOKEN);
                    if (node.type == NodeType.ERROR) return node;
                    return new ClassVarNode() { authority = authority, isstatic = isstatic, varName = varName, varType = varType, body = node };
                }
            }
            return new ErrorNode() { error = "类定义语法错误 在第" + token.line + "行，" + token.value };
        }
        ASTNode ReadNew()
        {
            Token token = tokens[index];
            if (token.type != TokenType.IDENTIFIER_TOKEN && token.type != TokenType.UNKNOWN_TOKEN)
            {
                return new ErrorNode() { error = "类定义语法错误 在第" + token.line + "行，" + token.value };
            }
            return null;
        }
        ASTNode ReadDefVar(string varType, string varName)
        {
            Token token = tokens[index];
            if (token.type == TokenType.END_TOKEN)
            {
                Next();
                return new VarNode() { varName = varName, varType = varType };
            }
            if (token.type == TokenType.SET_TOKEN)
            {
                Next();
                token = tokens[index];
                if (token.type == TokenType.NEW_TOKEN)
                {
                    Next(); 
                    var rn = ReadNew();
                    if (rn.type == NodeType.ERROR) return rn;
                    return new VarNode() { varName = varName, varType = varType, value = rn };
                }
                else if (token.type == TokenType.UNKNOWN_TOKEN || token.type == TokenType.NUMBER_TOKEN || token.type == TokenType.STRING_TOKEN || token.type == TokenType.BOOL_TOKEN)
                {
                    if (tokens[index + 1].type == TokenType.END_TOKEN)
                    {
                        Jump(2);
                        return new VarNode() { varName = varName, varType = varType, value = new ValueNode() { valueType = token.type, value = token.value } };
                    }
                    ASTNode node = ReadExpression(TokenType.END_TOKEN);
                    if (node.type == NodeType.ERROR) return node;
                    return new VarNode() {  varName = varName, varType = varType, value = node };
                }
                else
                {
                    ASTNode node = ReadExpression(TokenType.END_TOKEN);
                    if (node.type == NodeType.ERROR) return node;
                    return new VarNode() {  varName = varName, varType = varType, value = node };
                }
            }
            return new ErrorNode() { error = "类定义语法错误 在第" + token.line + "行，" + token.value };
        }
        ASTNode ReadDefFunc(string returnType, string funcName, TokenType authority, bool isstatic,  int depth)
        {
            FuncNode node = new FuncNode();
             var temp= ReadDefFuncArgs();
            if (temp.type == NodeType.ERROR) return temp;
            node.authority = authority;
            node.isstatic = isstatic;
            node.returnType = returnType;
            node.funcName = funcName;
            node.args = temp;
            Token token = tokens[index];
            if (token.type != TokenType.BIG_BRACKETS_LEFT_TOKEN) return new ErrorNode() { error = "定义function错误"+token.value+"在第"+token.line+"行" };
            temp= ReadBlock(depth + 1);
            if (temp.type == NodeType.ERROR) return temp;
            node.body = temp;
            return node;
        }
        ASTNode ReadDefFuncArgs()
        {
            BlockNode node = new BlockNode();
            Next();
            Token token = tokens[index];
            while (token.type!=TokenType.BRACKETS_RIGHT_TOKEN)
            {
                if (token.type == TokenType.UNKNOWN_TOKEN || token.type == TokenType.IDENTIFIER_TOKEN)
                {
                    string varType = ReadDefVarType();
                    token = tokens[index];
                    if(token.type!=TokenType.UNKNOWN_TOKEN)return new ErrorNode() { error = "参数定义错误 " + token.value + "在第" + token.line + "行" };
                    string varName = token.value;
                    node.targets.Add( new VarNode() { varType = varType, varName = varName });
                    Next();
                    token = tokens[index];
                    if (token.type == TokenType.COMMA_TOKEN)
                    {
                        Next();
                        token = tokens[index];
                    }
                    else if (token.type == TokenType.BRACKETS_RIGHT_TOKEN)
                    {
                        token = tokens[index];
                    }
                    else
                    {
                        return new ErrorNode() { error = "参数定义错误 " + token.value + "在第" + token.line + "行" };
                    }
                }
                else
                {
                    return new ErrorNode() { error = "参数定义错误 "+token.value+"在第"+token.line+"行" };
                }
            }
            Next();
            return node;
        }

        static ASTNode ReadDefEnum(TokenType authority,  int depth)
        {
            
            return null;
        }

        int GetOrder(string symbol)
        {
            switch (symbol)
            {

                case "=":
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "|=":
                case "&=":
                case "%=":
                    return 0;
                case "<<":
                case ">>":
                    return 1;
                case "+":
                case "-":
                    return 2;
                case "*":
                case "/":
                    return 3;
                case "&":
                case "|":
                    return 4;
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "==":
                case "!=":
                    return 5;
                default:
                    return -1;
            }
        }
        //方法调用或者获取数组字典元素 循环 如 A(x)(x)(x)(x) A(x)[x][x][x](x) A[x][x](x)(x)
        ASTNode ReadCallFuncOrGetEmLoop() {
            Token token = tokens[index];
            Next();
            ASTNode node = null;
            while (tokens[index].type==TokenType.BRACKETS_LEFT_TOKEN || tokens[index].type == TokenType.MIDDLE_BRACKETS_LEFT_TOKEN)
            {
                if(tokens[index].type == TokenType.BRACKETS_LEFT_TOKEN)
                {
                    Next();
                    if (node == null)
                    {
                        var args = ReadCallArgs();
                        if (args.type == NodeType.ERROR) return args;
                        node = new CallNode() { funcName = token.value, args = args };
                    }
                    else
                    {
                        var args = ReadCallArgs();
                        if (args.type == NodeType.ERROR) return args;
                        var temp = new CalculateNode() { left = node, symbol = "call", right = new CallNode() { funcName = token.value, args = args } };
                        node = temp;
                    }
                }
                else
                {
                    Next();
                    if (node == null)
                    {
                        node = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = "get", right = ReadExpression(TokenType.MIDDLE_BRACKETS_RIGHT_TOKEN) };
                    }
                    else
                    {
                        var temp = new CalculateNode() { left = node, symbol = "get", right = ReadExpression(TokenType.MIDDLE_BRACKETS_RIGHT_TOKEN) };
                        node = temp;
                    }
                }
                
            }
            return node;
        }

        ASTNode ReadCallArgs()
        {
            BlockNode node = new BlockNode();
            Token token = tokens[index];
            while (token.type!=TokenType.BRACKETS_RIGHT_TOKEN)
            {
              var n=  ReadExpressionTwoParam(TokenType.COMMA_TOKEN, TokenType.BRACKETS_RIGHT_TOKEN);
              if (n.type == NodeType.ERROR) return n;
                node.targets.Add(n);
                if (tokens[index - 1].type == TokenType.BRACKETS_RIGHT_TOKEN)
                {
                    Before();
                }
                token = tokens[index];
            }
            Next();
            return node;
        }

        ASTNode ReadExpressionTwoParam(TokenType endType1, TokenType endType2)
        {
            Token token = tokens[index];
            CalculateNode lastNode = null;
            while (token.type != endType1&& token.type != endType2)
            {
                switch (token.type)
                {
                    case TokenType.UNKNOWN_TOKEN:
                      
                        if (tokens[index + 1].type == TokenType.DOT_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = ".", right = null };
                            }
                            else
                            {
                                lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                var n = new CalculateNode() { left = lastNode, symbol = ".", right = null };
                                lastNode.parent = n;
                                lastNode = n;
                            }
                            Jump(2);
                        }
                        else if (tokens[index + 1].type == TokenType.BRACKETS_LEFT_TOKEN || tokens[index + 1].type == TokenType.MIDDLE_BRACKETS_LEFT_TOKEN)
                        {
                            var temp = ReadCallFuncOrGetEmLoop();
                            if (tokens[index].type == endType1 || tokens[index].type == endType2)
                            {
                                if (lastNode == null)
                                {
                                    Next();
                                    return temp;
                                }
                                lastNode.right = temp;
                                var p = lastNode;
                                while (p.parent != null)
                                {
                                    p = p.parent;
                                }
                                Next();
                                return p;
                            }
                            else
                            {
                                if (lastNode == null)
                                {
                                    var n = new CalculateNode() { left = temp, symbol = tokens[index].value, right = null };
                                    lastNode = n;
                                }
                                else
                                {
                                   
                                    var order = GetOrder(tokens[index].value);
                                    if (order == -1)
                                    {
                                        return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                                    }
                                    if (lastNode.parent == null)
                                    {
                                        lastNode.right = temp;
                                        var n = new CalculateNode() { left = lastNode, symbol = tokens[index].value, right = null };
                                        lastNode.parent = n;
                                        lastNode = n;
                                    }
                                    else if (GetOrder(lastNode.parent.symbol) > order)
                                    {
                                        lastNode.right = temp;
                                        var n = new CalculateNode() { parent = lastNode.parent, left = lastNode, symbol = tokens[index].value, right = null };
                                        lastNode.parent = n;
                                        lastNode = n;
                                    }
                                    else
                                    {
                                        var n = new CalculateNode() { parent = lastNode, left = temp, symbol = tokens[index].value, right = null };
                                        lastNode.right = n;
                                        lastNode = n;
                                    }
                                }
                            }
                            Next();
                        }
                        else if (tokens[index + 1].type >= TokenType.LAND_TOKEN && tokens[index + 1].type <= TokenType.MXOR_TO_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };

                            }
                            else
                            {
                                if (GetOrder(lastNode.symbol) >= GetOrder(tokens[index + 1].value))
                                {
                                    lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                    var n = new CalculateNode() { left = lastNode, symbol = tokens[index + 1].value, right = null };
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n = new CalculateNode() { parent = lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                            Jump(2);
                        }
                        else if (tokens[index + 1].type == endType1|| tokens[index+1].type == endType2)
                        {
                            if (lastNode == null)
                            {
                                Jump(2);
                               return new ValueNode() { valueType = token.type, value = token.value };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        break;
                    case TokenType.IDENTIFIER_TOKEN:
                        if (tokens[index + 1].type != TokenType.DOT_TOKEN)
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }
                        lastNode = new CalculateNode() { parent = lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = ".", right = null };
                        if (lastNode.parent != null)
                        {
                            lastNode.parent.right = lastNode;
                        }
                        Jump(2);
                        break;
                    case TokenType.IF_TOKEN:
                        break;
                    case TokenType.SWITCH_TOKEN:
                        break;
                    case TokenType.FOR_TOKEN:
                        break;
                    case TokenType.WHILE_TOKEN:
                        break;
                    case TokenType.DO_TOKEN:
                        break;
                    case TokenType.RETURN_TOKEN:
                        break;
                    case TokenType.NEW_TOKEN:
                        break;
                    case TokenType.NULL_TOKEN:
                        if (tokens[index + 1].type == endType1|| tokens[index + 1].type == endType2)
                        {
                            if (lastNode == null)
                            {
                                return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                            }
                            if (lastNode.symbol != "=")
                            {
                                return new ErrorNode() { error = "语法错误 null只能作为赋值右值使用 在第" + token.line + "行" };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        else
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }
                    case TokenType.NUMBER_TOKEN:
                    case TokenType.STRING_TOKEN:
                    case TokenType.BOOL_TOKEN:
                        if (tokens[index + 1].type == endType1|| tokens[index + 1].type == endType2)
                        {
                            if (lastNode == null)
                            {
                                Jump(2);
                                return new ValueNode() { valueType = token.type, value = token.value };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        else if (tokens[index + 1].type >= TokenType.LAND_TOKEN && tokens[index + 1].type <= TokenType.MXOR_TO_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };

                            }
                            else
                            {
                                if (GetOrder(lastNode.symbol) >= GetOrder(tokens[index + 1].value))
                                {
                                    lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                    var n = new CalculateNode() { left = lastNode, symbol = tokens[index + 1].value, right = null };
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n = new CalculateNode() { parent = lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                            Jump(2);
                        }
                        else
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }

                        break;
                    case TokenType.BRACKETS_LEFT_TOKEN:
                        Next();
                        var t_exp = ReadExpression(TokenType.BRACKETS_RIGHT_TOKEN);
                        if (t_exp.type == NodeType.ERROR)
                        {
                            return t_exp;
                        }
                        token = tokens[index];
                        if (token.type == endType1|| token.type == endType2)
                        {
                            if (lastNode == null)
                            {
                                Next();
                                return t_exp;
                            }
                            else
                            {
                                lastNode.right = t_exp;
                                Next();
                                return lastNode;
                            }
                        }
                        else
                        {
                            if (lastNode == null)
                            {
                                if (token.type >= TokenType.LAND_TOKEN && token.type <= TokenType.MXOR_TO_TOKEN)
                                {
                                    lastNode = new CalculateNode() { left = t_exp, symbol = token.value };
                                }
                            }
                            else
                            {
                                if (GetOrder(lastNode.symbol) >= GetOrder(token.value))
                                {
                                    lastNode.right = t_exp;
                                    var n = new CalculateNode() { left = lastNode, symbol = tokens[index].value, right = null };
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n = new CalculateNode() { parent = lastNode, left = t_exp, symbol = tokens[index].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                        }
                        Next();
                        break;
                    default:
                        return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                }
                token = tokens[index];
            }
            Next();
            var result = lastNode;
            while (result.parent != null)
            {
                result = result.parent;
            }
            return result;
        }

        ASTNode ReadExpression(TokenType endType)
        {
            Token token = tokens[index];
            CalculateNode lastNode = null;
            while (token.type!=endType)
            {
                switch (token.type)
                {
                    case TokenType.UNKNOWN_TOKEN:
                        if(tokens[index + 1].type == TokenType.DOT_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = ".", right = null };
                            }
                            else
                            {
                                lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                var n = new CalculateNode() { left = lastNode, symbol = ".", right = null };
                                lastNode.parent = n;
                                lastNode = n;
                            }
                            Jump(2);
                        }
                        else if (tokens[index + 1].type == TokenType.BRACKETS_LEFT_TOKEN || tokens[index + 1].type == TokenType.MIDDLE_BRACKETS_LEFT_TOKEN)
                        {
                            var temp = ReadCallFuncOrGetEmLoop();
                            if (tokens[index].type == endType)
                            {
                                if (lastNode == null)
                                {
                                    Next();
                                    return temp;
                                }
                                lastNode.right = temp;
                                var p = lastNode;
                                while (p.parent != null)
                                {
                                    p = p.parent;
                                }
                                Next();
                                return p;
                            }
                            else
                            {
                                if (lastNode == null)
                                {
                                    var n = new CalculateNode() { left = temp, symbol = tokens[index].value, right = null };
                                    lastNode = n;
                                }
                                else
                                {
                                   
                                    var order = GetOrder(tokens[index].value);
                                    if (order == -1)
                                    {
                                        return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                                    }
                                    if (lastNode.parent == null)
                                    {
                                        lastNode.right = temp;
                                        var n = new CalculateNode() { left = lastNode, symbol = tokens[index].value};
                                        lastNode.parent = n;
                                        lastNode = n;
                                    }
                                    else if (GetOrder(lastNode.symbol) > order)
                                    {
                                        lastNode.right = temp;
                                        var n = new CalculateNode() { parent = lastNode.parent, left = lastNode, symbol = tokens[index].value, right = null };
                                        lastNode.parent = n;
                                        lastNode = n;
                                    }
                                    else
                                    {
                                        var n = new CalculateNode() { parent = lastNode, left = temp, symbol = tokens[index].value, right = null };
                                        lastNode.right = n;
                                        lastNode = n;
                                    }
                                }
                            }
                            Next();
                        }
                        else if (tokens[index + 1].type >= TokenType.LAND_TOKEN && tokens[index + 1].type <= TokenType.MXOR_TO_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };
                                
                            }
                            else
                            {
                               if( GetOrder(lastNode.symbol)>=GetOrder(tokens[index+1].value))
                                {
                                    lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                    var n = new CalculateNode() {parent=lastNode.parent, left = lastNode, symbol = tokens[index+1].value, right = null };
                                    lastNode.parent.right = n;
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n =  new CalculateNode() {parent=lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                            Jump(2);
                        }
                        else if(tokens[index + 1].type == endType)
                        {
                            if (lastNode == null)
                            {
                                Next();
                                return new ValueNode() { valueType = token.type, value = token.value };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        break;
                    case TokenType.IDENTIFIER_TOKEN:
                        if (lastNode != null)
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }
                        if (tokens[index + 1].type != TokenType.DOT_TOKEN)
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }
                        lastNode= new CalculateNode() { parent = lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = ".", right = null };
                        Jump(2);
                        break;
                    case TokenType.IF_TOKEN:
                        break;
                    case TokenType.SWITCH_TOKEN:
                        break;
                    case TokenType.FOR_TOKEN:
                        break;
                    case TokenType.WHILE_TOKEN:
                        break;
                    case TokenType.DO_TOKEN:
                        break;
                    case TokenType.RETURN_TOKEN:
                        break;
                    case TokenType.NEW_TOKEN:
                        break;
                    case TokenType.NULL_TOKEN:
                        if (tokens[index + 1].type == endType)
                        {
                            if (lastNode == null)
                            {
                                return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                            }
                            if (lastNode.symbol != "=")
                            {
                                return new ErrorNode() { error = "语法错误 null只能作为赋值右值使用 在第" + token.line + "行" };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        else
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }
                    case TokenType.NUMBER_TOKEN:
                    case TokenType.STRING_TOKEN:
                    case TokenType.BOOL_TOKEN:
                        if (tokens[index + 1].type == endType)
                        {
                            if (lastNode == null)
                            {
                                return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                            }
                            lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                            var p = lastNode;
                            while (p.parent != null)
                            {
                                p = p.parent;
                            }
                            Jump(2);
                            return p;
                        }
                        else if (tokens[index + 1].type >= TokenType.LAND_TOKEN && tokens[index + 1].type <= TokenType.MXOR_TO_TOKEN)
                        {
                            if (lastNode == null)
                            {
                                lastNode = new CalculateNode() { left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };

                            }
                            else
                            {
                                if (GetOrder(lastNode.symbol) >= GetOrder(tokens[index + 1].value))
                                {
                                    lastNode.right = new ValueNode() { valueType = token.type, value = token.value };
                                    var n = new CalculateNode() { left = lastNode, symbol = tokens[index+1].value, right = null };
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n = new CalculateNode() { parent = lastNode, left = new ValueNode() { valueType = token.type, value = token.value }, symbol = tokens[index + 1].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                            Jump(2);
                        }
                        else
                        {
                            return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                        }

                        break;
                    case TokenType.NOTE_TOKEN:
                        break;
                    case TokenType.BRACKETS_LEFT_TOKEN:
                        Next();
                         var t_exp= ReadExpression(TokenType.BRACKETS_RIGHT_TOKEN);
                        if (t_exp.type == NodeType.ERROR)
                        {
                            return t_exp;
                        }
                        token = tokens[index];
                        if (token.type == endType)
                        {
                            if (lastNode == null)
                            {
                                Next();
                                return t_exp;
                            }
                            else
                            {
                                lastNode.right = t_exp;
                                Next();
                                return lastNode;
                            }
                        }
                        else
                        {
                            if (lastNode == null)
                            {
                                if (token.type >= TokenType.LAND_TOKEN && token.type <= TokenType.MXOR_TO_TOKEN)
                                {
                                    lastNode = new CalculateNode() { left = t_exp, symbol = token.value };
                                }
                            }
                            else
                            {
                                if (GetOrder(lastNode.symbol) >= GetOrder(token.value))
                                {
                                    lastNode.right = t_exp;
                                    var n = new CalculateNode() { left = lastNode, symbol = tokens[index].value, right = null };
                                    lastNode.parent = n;
                                    lastNode = n;
                                }
                                else
                                {
                                    var n = new CalculateNode() { parent = lastNode, left = t_exp, symbol = tokens[index].value, right = null };
                                    lastNode.right = n;
                                    lastNode = n;
                                }
                            }
                        }

                       
                        Next();
                        break;
                    default:
                        return new ErrorNode() { error = "语法错误" + token.value + "在第" + token.line + "行" };
                }
                token = tokens[index];
            }
            Next();
            var result = lastNode;
            while (result.parent != null)
            {
                result = result.parent;
            }
            return result;
        }
        //读取块或者数组定义 如a{}  a[]
        ErrorNode ReadBlockUnknowOrIdent(BlockNode node)
        {
            ASTNode child = null;
            Token token = tokens[index];
            if ( tokens[index + 1].type == TokenType.UNKNOWN_TOKEN)
            {
                string varType = token.value;
                Next();
                token = tokens[index];
                Next();
                child = ReadDefVar(varType, token.value);
                if (child.type == NodeType.ERROR) return child as ErrorNode;
                node.targets.Add(child);
                return null;
            }

            //跟着的是成对中括号 说明是数组定义
            if(tokens[index+1].value=="["&& tokens[index + 2].value == "]")
            {
                string type=ReadDefVarType();
                token = tokens[index];
                if (token.type != TokenType.UNKNOWN_TOKEN) return new ErrorNode() { error = "变量定义错误 错误的变量名"+token.value+"在第"+token.line+"行" };
                Next();
                child = ReadDefVar(type, token.value);
                if (child.type == NodeType.ERROR) return child as ErrorNode;
                node.targets.Add(child);
                return null;
            }


            return null;
        }
        
        ASTNode ReadBlock(int depth)
        {
            BlockNode node = new BlockNode();
            Next();
            Token token = tokens[index];
            while (token.type!=TokenType.BIG_BRACKETS_RIGHT_TOKEN)
            {
                ASTNode temp=null;
                switch (token.type)
                {
                    case TokenType.ENUM_TOKEN:
                        Next();
                        temp=ReadDefEnum(TokenType.PRIVATE_TOKEN, depth);
                        break;
                    case TokenType.PUBLIC_TOKEN:
                    case TokenType.PRIVATE_TOKEN:

                        if (tokens[index + 1].type == TokenType.STATIC_TOKEN)
                        {
                            Jump(2);
                            temp=ReadAuthority(token.type, true, depth);
                        }
                        else
                        {
                            Next();
                            temp=ReadAuthority(token.type, false, depth);
                        }
                        break;
                    case TokenType.STATIC_TOKEN:
                        Next();
                        token = tokens[index];
                        if (token.type == TokenType.PUBLIC_TOKEN || token.type == TokenType.PRIVATE_TOKEN)
                        {
                            Next();
                            temp=ReadAuthority(token.type, true, depth);
                        }
                        else
                        {
                            temp=ReadAuthority(token.type, false, depth);
                        }
                        break;
                    case TokenType.IDENTIFIER_TOKEN:
                    case TokenType.UNKNOWN_TOKEN:
                        if (depth == 1)
                        {
                           temp= ReadAuthority(TokenType.PRIVATE_TOKEN, false, depth);
                        }
                        else
                        {
                           temp= ReadBlockUnknowOrIdent(node);
                            if(temp!=null)
                            return temp;
                        }
                        break;
                    case TokenType.NOTE_TOKEN:
                        return new ErrorNode() { type = NodeType.ERROR, error = "暂未支持注释" + token.value + "在第" + token.line + "行" };
                    default:
                        return new ErrorNode() { type = NodeType.ERROR, error = "语法错误" + token.value + "在第" + token.line + "行" };
                }
                if (temp != null) { if (temp.type == NodeType.ERROR) return temp; else node.targets.Add(temp); }
                token = tokens[index];
            }
            Next();
            return node;
        }
    }

    
}
