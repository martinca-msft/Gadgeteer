Imports GT = Gadgeteer
Imports GTM = Gadgeteer.Modules

Namespace $safeprojectname$
    Partial Public Class Program

        ' This is run when the mainboard is powered up or reset. 
        Public Sub ProgramStarted()
            '*******************************************************************************************
            ' Hardware modules added in the Program.gadgeteer designer view are used by typing 
            ' their name followed by a period, e.g.  button.  or  camera.
            '
            ' Many hardware modules generate useful events. To set up actions for those events, use the 
            ' left dropdown box at the top of this code editing window to choose a hardware module, then
            ' use the right dropdown box to choose the event - an event handler will be auto-generated.
            '*******************************************************************************************/

            ' Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started")
        End Sub

        ' If you want to do something periodically, declare a GT.Timer by uncommenting the below line
        '   and then use the dropdown boxes at the top of this window to generate a Tick event handler.
        ' Dim WithEvents timer As GT.Timer = new GT.Timer(1000)  ' every second (1000ms)

    End Class
End Namespace