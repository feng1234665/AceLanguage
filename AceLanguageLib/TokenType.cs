using System;
using System.Collections.Generic;
using System.Text;

namespace AceLanguage
{
 public   enum TokenType
    {
        NO_STATUS = 0,// 初始化状态
        UNKNOWN_TOKEN,// 未知token
        ERROR_TOKEN,// 语法错误的token
        IDENTIFIER_TOKEN,
                      // key words
        IF_TOKEN, ELIF_TOKEN, ELSE_TOKEN, SWITCH_TOKEN, CASE_TOKEN, DEFAULT_TOKEN,//逻辑关键字
        FOR_TOKEN, WHILE_TOKEN, DO_TOKEN, CONTINUE_TOKEN, BREAK_TOKEN,//循环关键字
        RETURN_TOKEN,//返回
        ENUM_TOKEN,//枚举
        USING_TOKEN,//引用
        IMPORT_TOKEN, EXPORT_TOKEN, AS_TOKEN,//引用、导出、转换
        DELETE_TOKEN,//删除
        CLASS_TOKEN, PUBLIC_TOKEN, PRIVATE_TOKEN, NEW_TOKEN, STATIC_TOKEN, OPERATOR_TOKEN, EXTENDS_TOKEN,//类/权限关键之
        VOID_TOKEN,NULL_TOKEN, UNDEFINED_TOKEN, DEBUGGER_TOKEN,//空指针关键字
        NUMBER_TOKEN, STRING_TOKEN,BOOL_TOKEN,//数字、字符串,布尔值
        // 注释token
        NOTE_TOKEN,
        // 符号token
        UNKNOWN_OP_TOKEN,
        SET_TOKEN, COLON_TOKEN,//=，:
        EQUAL_TOKEN, NOT_EQUAL_TOKEN, LEQUAL_TOKEN, MEQUAL_TOKEN,//==，！=，<=,>=
        LESS_TOKEN, MORE_TOKEN,//<,>
        BRACKETS_LEFT_TOKEN, BRACKETS_RIGHT_TOKEN,//(,)
        MIDDLE_BRACKETS_LEFT_TOKEN, MIDDLE_BRACKETS_RIGHT_TOKEN,//[,]
        BIG_BRACKETS_LEFT_TOKEN, BIG_BRACKETS_RIGHT_TOKEN,//{,}
        DOT_TOKEN, COMMA_TOKEN,//. ,
        LAND_TOKEN, LOR_TOKEN, LNOT_TOKEN,// && || !
        MAND_TOKEN, MOR_TOKEN, MXOR_TOKEN, MNOT_TOKEN,//&,|,^,~
        ADD_TOKEN, SUB_TOKEN, MUL_TOKEN, DIV_TOKEN, MOD_TOKEN, // + - * / % 
        SLEFT_TOKEN, SRIGHT_TOKEN,//<< >>
        ADD_TO_TOKEN, SUB_TO_TOKEN, MUL_TO_TOKEN, DIV_TO_TOKEN, MOD_TO_TOKEN,//+= -= *= /= %=
        SLEFT_TO_TOKEN, SRIGHT_TO_TOKEN,//<<= >>=
        MAND_TO_TOKEN, MOR_TO_TOKEN, MXOR_TO_TOKEN,//&= |= ^=
        END_TOKEN,// 语句结束 ；
        PROGRAM_END
    }
}
