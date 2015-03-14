<%@ Control Language="C#" AutoEventWireup="true" Inherits="Jhu.Graywulf.SciDrive.ImportTablesFromSciDriveForm" Codebehind="ImportTablesFromSciDriveForm.ascx.cs" %>

<p>
    Specify a file name to pull from SciDrive
</p>
<table class="FormTable">
    <tr>
        <td class="FormLabel" style="width: 50px">
            <asp:Label runat="server" ID="uriLabel">Path:</asp:Label>&nbsp;&nbsp;
        </td>
        <td class="FormField" style="width: 420px">
            <asp:TextBox runat="server" ID="uri" CssClass="FormField" Width="420px" Text="first_container/myfile.csv" />
        </td>
    </tr>
</table>
