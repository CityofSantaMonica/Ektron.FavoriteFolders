<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FavoriteFolders.ascx.cs" Inherits="FavoriteFoldersWidget" %>

<div style="overflow: hidden;">
    <asp:MultiView ID="ViewSet" runat="server" ActiveViewIndex="0">

        <asp:View ID="uxWidgetView" runat="server">

            <asp:Label ToolTip="No Records" ID="NoRecordsLabel" Visible="false" runat="server">Click the check at the top right to add folders to this list</asp:Label>
                
            <div class="ektronPageGrid" style="overflow:auto;">
                <asp:DataGrid ID="DataGrid" runat="server" Width="100%" Visible="false" AutoGenerateColumns="False" EnableViewState="False" GridLines="None" CssClass="ektronGrid ektronBorder" style="display: table">
                    <HeaderStyle CssClass="title-header" />
                    <Columns>
                        <asp:BoundColumn DataField="PathLink" HeaderText="Folder (click to view folder contents)" />
                    </Columns>
                </asp:DataGrid>
            </div>
        </asp:View>

        <asp:View ID="uxEditView" runat="server">
            
            <div id="<%=ClientID%>_edit">
                
                Select Folders to add to Favorites:
                <br />
                <asp:TreeView ID="FolderTreeView" runat="server" ShowCheckBoxes="All"></asp:TreeView>
                <br />

                <asp:Button  ID="uxCancelButton" runat="server" Text="Cancel" ToolTip="Cancel" OnClick="CancelButton_Click" />
                &nbsp;&nbsp; 
                <asp:Button  ID="uxSaveButton" runat="server" Text="Save" ToolTip="Save" OnClick="SaveButton_Click" />
            </div>

        </asp:View>

    </asp:MultiView>
</div>
