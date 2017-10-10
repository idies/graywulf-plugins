<%@ Control Language="C#" AutoEventWireup="true" Inherits="Jhu.Graywulf.SciDrive.ExportTablesToSciDriveForm" Codebehind="ExportTablesToSciDriveForm.ascx.cs" %>

<p>
    Specify a file on SciDrive
</p>
<table class="FormTable">
    <tr>
        <td class="FormLabel" style="width: 50px">
            <asp:Label runat="server" ID="uriLabel">Path:</asp:Label>&nbsp;&nbsp;
        </td>
        <td class="FormField" style="width: 420px">
            <asp:TextBox runat="server" ID="uri" CssClass="FormField" Width="420px" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="uri" Display="Dynamic"
                ErrorMessage="<br />The URI field is required" />
            <asp:RegularExpressionValidator ID="uriFormatValidator" runat="server" ControlToValidate="uri" Display="Dynamic"
                ErrorMessage="<br />Invalid SciDrive path" />
        </td>
    </tr>
</table>
