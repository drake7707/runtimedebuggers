Public Class Class1


    Public Sub TestWithExceptionFilter()

        Try
            Console.WriteLine("foo")

        Catch ex As Exception When DateTime.Now.Second Mod 2 = 0
            Console.WriteLine("meh")
        End Try
    End Sub
End Class
