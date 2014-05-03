Public Class Class1


    Public Shared Sub TestWithExceptionFilter()

        Try
            WriteText("Try")

        Catch ex As Exception When WriteText("Filter")
            WriteText("Catch")
        Finally
            WriteText("Finally")
        End Try
    End Sub

    Public Shared Function WriteText(text As String) As Boolean
        Return True

    End Function
End Class
