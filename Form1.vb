Imports System.Windows.Forms 'Needed for methodInvoker

Public Class Form1
    Event tagSaveBPM(ByVal oldValue As String, ByVal newValue As String)

    Dim myCount, bpm, currSec, StartSec, oldBPM, strBpm, avgChg As Double 'BPM Math Variables
    Dim PavgChg As String
    Dim StartDate, currDate As Date
    'Dim TS As New TimeSpan

    Public Sub Btn_Tap()
        'Debug.Print("Tapped!")
        currDate = Now()
        strBpm = 0
        If myCount > 1 Then         'Only attempt after 2nd Tap (on 1st tap it is either "1st Tap" or "Reset")
            Try
                strBpm = Convert.ToDecimal(TextBox1.Text)
            Catch ex As Exception   'Catch Format Error when converting String to Decimal
            End Try
        End If

        If myCount = 0 Then
            TextBox1.Text = "1st Tap"
            StartDate = Now()
        Else

            bpm = (myCount * 60000) / (currDate.Subtract(StartDate).TotalMilliseconds)
            'Debug.Print("Math for Date is" + currDate.Subtract(StartDate).TotalMilliseconds.ToString)
            'Debug.Print("myCount is " + myCount.ToString)
            'Debug.Print("bpm is " + bpm.ToString())

            'THE FOLLOWING was used for studying the accuracy of the most recent Tap compared to the average
            '  bpm_last = (1 * 60000 / (currDate.Subtract(OneCountAgo).TotalMilliseconds))
            '  OneCountAgo = Now()
            '  Debug.Print("bpm is " + bpm.ToString() + " / bpm_last is " + bpm_last.ToString())

            avgChg = Math.Round((bpm - oldBPM) * 10) / 10   'Change in bpm from this click rounded to tenths (should tend to 0.0)
            oldBPM = bpm                                    'Update oldBPM for next click action

            If (avgChg >= 0) Then
                PavgChg = "+" & avgChg.ToString()
            Else
                PavgChg = avgChg.ToString()                 'PavgChg is Change in the Average BPM (Could Display This)
            End If

            TextBox1.Text = (Math.Round(bpm)).ToString()    'Round result to an integer (Rounding is a personal preference)

            'Check Usability of Modifier Buttons
            If (bpm > 400) Then
                Button4.Enabled = False             'Disable Double Button
            Else
                Button4.Enabled = True              'Enable Double Button
            End If

            If (bpm < 40) Then
                Button5.Enabled = False             'Disable Half Button
            Else
                Button5.Enabled = True              'Enable Half Button
            End If

        End If

        myCount = myCount + 1

        Label1.Text = "TAPS: " & myCount.ToString   'Update the TAPS Label
        Label2.Text = "DIFF: " & PavgChg            'Update the DIFF Label

        If (((avgChg > 75) Or (avgChg < -75)) And myCount > 2) Then
            Debug.Print("Starting Over")
            TextBox1.Text = "Reset"
            StartDate = Now()
            myCount = 1
            bpm = 0
            oldBPM = 0

            Button4.Enabled = True          'Enable Double Button
            Button5.Enabled = True          'Enable Half Button
            Label1.Text = "TAPS: 0"         'Update the TAPS Label
            Label2.Text = "DIFF: 0"         'Update the DIFF Label
        End If
    End Sub

    Public Sub Btn_Reset()


        'Invoking is required since this sub is called whenever the Now Playing Song changes (which is a different Thread)
        If Me.InvokeRequired Then
            Me.Invoke(New MethodInvoker(AddressOf Btn_Reset))
        Else

            Debug.Print("Form Reset")
            myCount = 0
            bpm = 0
            oldBPM = 0

            Button4.Enabled = True          'Enable Double Button
            Button5.Enabled = True          'Enable Half Button
            Label1.Text = "TAPS: 0"         'Update the TAPS Label
            Label2.Text = "DIFF: 0"         'Update the DIFF Label
            If Plugin.strBPM = Nothing Then
                Plugin.strBPM = "0"
            End If
            TextBox1.Text = Plugin.strBPM
        End If

    End Sub

    Public Sub Btn_Save()
        Dim newValue As String
        newValue = TextBox1.Text
        Dim oldValue As String = Plugin.strBPM
        Plugin.strBPM = newValue
        RaiseEvent tagSaveBPM(oldValue, newValue)

        myCount = 0      'Reset the values
        bpm = 0          'Reset the values
        oldBPM = 0       'Reset the values
        Me.Hide()
    End Sub



    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        'Tap Button
        Btn_Tap()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        'Reset Button
        Btn_Reset()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        'Save Button
        Btn_Save()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Debug.Print("Just Loaded Form1")
        Btn_Reset()
    End Sub



    Private Sub Btn_Double(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        'Double BPM Button
        myCount = 0
        Label1.Text = "TAPS: 0"         'Update the TAPS Label
        Label2.Text = "DIFF: 0"         'Update the DIFF Label

        strBpm = 0
        Try
            strBpm = Convert.ToDecimal(TextBox1.Text)
        Catch ex As Exception            'Catch Format Error when converting String to Decimal
        End Try
        bpm = strBpm * 2

        'Check Usability of Modify Button
        If (bpm > 400) Then
            Button4.Enabled = False 'Disable Double Button
        End If

        TextBox1.Text = (Math.Round((bpm) * 10) / 10).ToString 'Update with Value rounded to Tenth
        Button5.Enabled = True  'Enable Half Button
    End Sub

    Private Sub Btn_Half(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        'Half BPM Button
        myCount = 0
        Label1.Text = "TAPS: 0"         'Update the TAPS Label
        Label2.Text = "DIFF: 0"         'Update the DIFF Label

        strBpm = 0
        Try
            strBpm = Convert.ToDecimal(TextBox1.Text)
        Catch ex As Exception            'Catch Format Error when converting String to Decimal
        End Try

        bpm = strBpm / 2

        'Check Usability of Modify Button
        If (bpm < 40) Then
            Button5.Enabled = False 'Disable Half Button
        End If

        TextBox1.Text = (Math.Round((bpm) * 10) / 10).ToString 'Update with Value rounded to Tenth

        Button4.Enabled = True  'Enable Double Button
    End Sub

    Private Sub Form1_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Me.Hide()               'Not destroyed, just hidden. This inheirently keeps the same position for the pop-up
        e.Cancel = True
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        'Debug.Print("TB1 TextChanged")
    End Sub



End Class
