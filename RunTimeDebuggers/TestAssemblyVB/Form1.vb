Imports System.Windows.Forms

Public Class Form1

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        MessageBox.Show("Boop")

    End Sub

    Private Sub Button1_Enter(sender As System.Object, e As System.EventArgs) Handles Button1.Enter
        MessageBox.Show("Boopboop")
    End Sub
End Class