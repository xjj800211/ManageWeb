﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Role_Info.aspx.cs" Inherits="RM.Web.RMBase.SysRole.Role_Info" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>系统角色设置详细</title>
    <link href="/Themes/Styles/Site.css" rel="stylesheet" type="text/css" />
    <script src="/Themes/Scripts/jquery-1.8.2.min.js" type="text/javascript"></script>
    <script src="/Themes/Scripts/TreeTable/jquery.treeTable.js" type="text/javascript"></script>
    <link href="/Themes/Scripts/TreeTable/css/jquery.treeTable.css" rel="stylesheet"
        type="text/css" />
    <script src="/Themes/Scripts/FunctionJS.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            divresize(63);
            FixedTableHeader("#dnd-example", $(window).height() - 91);
            $("#dnd-example").treeTable({
                initialState: "expanded" //collapsed 收缩 expanded展开的
            });
            $('input[type="checkbox"]').attr('disabled', 'disabled');
        })
        //面板切换
        function TabPanel(id) {
            if (id == 1) {
                $("#div_RoleRight").show();
                $("#div_UserRole").hide();
            } else if (id == 2) {
                $("#div_RoleRight").hide();
                $("#div_UserRole").show();
                //固定表头
                //FixedTableHeader("#table1", $(window).height() - 94);
            }
        } 
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <input id="item_hidden" type="hidden" runat="server" />
    <div class="btnbartitle">
        <div>
            所属角色【<%=_Roles_Name.ToString()%>】
        </div>
    </div>
    <div class="btnbarcontetn">
        <div style="float: left;">
            <table style="padding: 0px; margin: 0px; height: 100%;" cellpadding="0" cellspacing="0">
                <tr>
                    <td id="menutab" style="vertical-align: bottom;">
                        <div id="tab0" class="Tabsel" onclick="GetTabClick(this);TabPanel(1)">
                            角色权限</div>
                        <div id="tab1" class="Tabremovesel" onclick="GetTabClick(this);TabPanel(2)">
                            角色成员</div>
                    </td>
                </tr>
            </table>
        </div>
        <div style="text-align: right">
            <a href="javascript:void(0)" title="返 回" onclick="back();" class="button green"><span
                class="icon-botton" style="background: url('/Themes/images/16/back.png') no-repeat scroll 0px 4px;">
            </span>返 回</a>
        </div>
    </div>
    <div class="div-body" id="div_RoleRight">
        <table class="example" id="dnd-example">
            <thead>
                <tr>
                    <td style="width: 200px; padding-left: 20px;">
                        URL菜单权限
                    </td>
                    <td style="width: 30px; text-align: center;">
                        图标
                    </td>
                    <td style="width: 20px; text-align: center;">
                        <label id="checkAllOff" title="全选">
                            &nbsp;</label>
                    </td>
                    <td>
                        操作按钮
                    </td>
                </tr>
            </thead>
            <tbody>
                <%=StrTree_Menu.ToString()%>
            </tbody>
        </table>
    </div>
    <div class="div-body" id="div_UserRole" style="display: none;">
        <table id="table1" class="grid">
            <thead>
                <tr>
                    <td style="width: 30px; text-align: center;">
                        序号
                    </td>
                    <td>
                        角色成员
                    </td>
                    <td>
                        所属部门
                    </td>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rp_Item" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td style="width: 30px; text-align: center;">
                                <%# Container.ItemIndex + 1%>
                            </td>
                            <td>
                                <%#Eval("User_Name")%>
                            </td>
                            <td>
                                <%#Eval("Organization_Name")%>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        <% if (rp_Item != null)
                           {
                               if (rp_Item.Items.Count == 0)
                               {
                                   Response.Write("<tr><td colspan='3' style='color:red;text-align:center'>没有找到您要的相关数据！</td></tr>");
                               }
                           } %>
                    </FooterTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
    </form>
</body>
</html>
