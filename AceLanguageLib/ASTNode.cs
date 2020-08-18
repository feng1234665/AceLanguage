using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AceLanguage
{
    public enum NodeType
    {
        ERROR=0,//错误表达式
        CLASS,//类型定义表达式
        FUNC,//方法定义表达式
        CALCULATE,//运算表达式
        VAR,//变量定义表达式
        BLOCK,//块表达式
        CALL,//方法调用表达式
        IF_ELSE,//if表达式
        FOR,//for循环表达式
        WHILE,//while循环表达式
        ENUM,//枚举
        VALUE,//值
        USING//导入
    }

  public  class ASTNode
    {
        //节点类型
        public NodeType type;

        public ASTNode() { }
        public ASTNode(NodeType type) { this.type = type; }
    }
    public class UsingNode : ASTNode
    {

        public UsingNode() : base(NodeType.USING) { }
        //错误信息
        public string path;
    }
    public class ErrorNode: ASTNode
    {

        public ErrorNode() :base(NodeType.ERROR) {}
        //错误信息
        public string error;
    }
    public class EnumNode : ASTNode
    {
        public EnumNode() : base(NodeType.ENUM) { }
        public TokenType AUTHORITY;//权限
        public string enumName;
        //枚举名称表
        public List<string> values=new List<string>();

    }
    public class ClassNode : ASTNode
    {
        public ClassNode() : base(NodeType.CLASS) { }
        public TokenType AUTHORITY;//权限
        public bool isstatic;//是否静态
        public string className;
        public ASTNode body;
    }

    public class ClassVarNode : ASTNode
    {
        public ClassVarNode() : base(NodeType.VAR) { }
        public TokenType authority;//权限
        public bool isstatic;//是否静态
        public string varType;
        public string varName;
        public ASTNode body=null;
    }
    public class FuncNode : ASTNode
    {
        public FuncNode() : base(NodeType.FUNC) { }
        public TokenType authority;//权限
        public bool isstatic;//是否静态
        public string returnType;
        public string funcName;
        public ASTNode args;
        public ASTNode body;
    }
    public class CalculateNode : ASTNode
    {
        public CalculateNode() : base(NodeType.CALCULATE) { }
        public CalculateNode parent=null;

        public ASTNode left=null;
        public string symbol="";
        public ASTNode right=null;
    }

    public class VarNode : ASTNode
    {
        public VarNode() : base(NodeType.VAR) { }
        public string varType;
        public string varName;
        public ASTNode value=null;
    }
    public class BlockNode : ASTNode
    {
        public BlockNode() : base(NodeType.BLOCK) { }
        public List<ASTNode> targets=new List<ASTNode>();
    }
    public class CallNode : ASTNode
    {
        public CallNode() : base(NodeType.CALL) { }
        public string funcName;
        public ASTNode args =null;
    }

    public class IfElseNode : ASTNode
    {
        public IfElseNode() : base(NodeType.IF_ELSE) { }
        //条件 如果条件为空 则表示这是一条else的语句 else是没有条件的 上一条的if没满足就直接进这里了
        public ASTNode condition;
        //逻辑主体
        public ASTNode body;
        //如果有else的情况 则next为else下的逻辑
        public ASTNode next;
    }
    public class ForNode : ASTNode
    {
        public ForNode() : base(NodeType.FOR) { }
        public ASTNode condition;
        public ASTNode body;
    }
    public class WhileNode : ASTNode
    {
        public WhileNode() : base(NodeType.WHILE) { }
        public ASTNode condition;
        public ASTNode body;
    }
    public class ValueNode : ASTNode
    {
        public ValueNode() : base(NodeType.VALUE) { }
        public TokenType valueType;
        public string value;
    }
}
