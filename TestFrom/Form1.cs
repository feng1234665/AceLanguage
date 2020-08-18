using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AceLanguage;

namespace TestFrom
{
    public partial class form1 : Form
    {
        public form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var s = File.ReadAllText("t.txt");
            var ts = AceLanguage.Lexical.Process(s);
            ASTNode node = AST.Process(ts.ToArray());
            if (node.type == NodeType.ERROR)
            {
              var er=  node as ErrorNode;
                MessageBox.Show("代码树解析错误" + er.error);
                return;
            }
            AddNew(node, treeView1.Nodes);
            treeView1.ExpandAll();
        }

        void AddNew(ASTNode node,TreeNodeCollection parent)
        {
            switch (node.type)
            {
                case NodeType.USING:
                    var unode = node as UsingNode;
                    parent.Add("导入包 " + unode.path);
                    break;
                case NodeType.CLASS:
                    var cnode= node as ClassNode;
                    var temp=parent.Add((cnode.isstatic ? "静态 " : "")+(cnode.AUTHORITY==TokenType.PUBLIC_TOKEN?"公开":"私有")+ " 类 "+ cnode.className);
                    var body = cnode.body as BlockNode;
                    foreach (var item in body.targets)
                    {
                        AddNew(item, temp.Nodes);
                    }
                    break;
                case NodeType.FUNC:
                    var fnode = node as FuncNode;
                    temp = parent.Add((fnode.isstatic ? "静态 " : "") + (fnode.authority == TokenType.PUBLIC_TOKEN ? "公开" : "私有") + " 函数 " + fnode.funcName);
                    var temp1 = temp.Nodes.Add("参数");
                    var args= fnode.args as BlockNode;
                    foreach (var item in args.targets)
                    {
                        AddNew(item, temp1.Nodes);
                    }
                    temp1 = temp.Nodes.Add("逻辑块");
                    body = fnode.body as BlockNode;
                    foreach (var item in body.targets)
                    {
                        AddNew(item, temp1.Nodes);
                    }
                    break;
                case NodeType.CALCULATE:
                    var canode = node as CalculateNode;
                    if (canode.symbol == "call")
                    {
                       var callnode1= canode.right as CallNode;
                        temp = parent.Add("调用方法 " + callnode1.funcName);
                        temp1 = temp.Nodes.Add("参数");
                        args = callnode1.args as BlockNode;
                        foreach (var item in args.targets)
                        {
                            AddNew(item, temp1.Nodes);
                        }
                        AddNew(canode.left, temp.Nodes);
                    }
                    else
                    {
                        temp = parent.Add("运算 "+canode.symbol);
                        AddNew(canode.left, temp.Nodes);
                        AddNew(canode.right, temp.Nodes);
                    }
                    break;
                case NodeType.VAR:
                    var vnode = node as VarNode;
                    if (vnode != null)
                    {
                        temp = parent.Add("变量 " + vnode.varType + " " + vnode.varName);
                        if (vnode.value != null)
                        {
                            AddNew(vnode.value, temp.Nodes);
                        }
                    }
                    else
                    {
                        var cvnode = node as ClassVarNode;
                        temp = parent.Add((cvnode.isstatic?"静态 ":"")+(cvnode.authority==TokenType.PUBLIC_TOKEN?"公开 ":"私有 ")+ "成员变量 " + cvnode.varType + " " + cvnode.varName);
                        if (cvnode.body != null)
                        {
                            AddNew(cvnode.body, temp.Nodes);
                        }
                    }
                    break;
                case NodeType.BLOCK:
                    var bnode = node as BlockNode;
                    temp= parent.Add("root");
                    foreach (var item in bnode.targets)
                    {
                        AddNew(item, temp.Nodes);
                    }
                    break;
                case NodeType.CALL:
                    var callnode = node as CallNode;
                    temp = parent.Add("调用方法 " + callnode.funcName);
                    temp1 = temp.Nodes.Add("参数");
                    args = callnode.args as BlockNode;
                    foreach (var item in args.targets)
                    {
                        AddNew(item, temp1.Nodes);
                    }
                    break;
                case NodeType.IF_ELSE:
                    break;
                case NodeType.FOR:
                    break;
                case NodeType.WHILE:
                    break;
                case NodeType.ENUM:
                    break;
                case NodeType.VALUE:
                    var vanode = node as ValueNode;
                    temp = parent.Add(/*GetVType(vanode.valueType)+ " " +*/ vanode.value);
                    break;
            }
        }

        string GetVType(TokenType type)
        {
            switch (type)
            {
                case TokenType.UNKNOWN_TOKEN:
                    return "userdata";
            
                case TokenType.NULL_TOKEN:
                    return "空对象";
                case TokenType.NUMBER_TOKEN:
                    return "数值";
                case TokenType.STRING_TOKEN:
                    return "字符串";
                case TokenType.BOOL_TOKEN:
                    return "布尔值";
                default:
                    return "错误类型";
            }
        }
    }
}
