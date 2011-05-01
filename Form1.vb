Imports System
Imports System.IO
Imports System.Net
Imports System.Globalization
Imports System.Threading

Public Class frmMain

    Dim filetext As String

    'Private Sub loadWebpage(ByVal addy As String)
    '    Dim p As New Process()
    '    Dim psi As New ProcessStartInfo(addy)
    '    p.StartInfo = psi
    '    p.Start()
    'End Sub

    Private Sub twitterAPIButton()
        Dim oFile As System.IO.File
        Dim oRead As System.IO.StreamReader
        Dim counter As Integer = 1
        Dim retrycounter As Integer = 0
        Dim s As String


        ListBox1.Items.Clear()

        While (True)

            filetext = ""
            Try
tryagain:
                If retrycounter = 5 Then
                    btnSearch.Enabled = True
                    lblStatus.Text = "Status: Connection error"
                    Exit Sub
                End If
                My.Computer.Network.DownloadFile(buildURL() & counter.ToString, "WineList.txt", "", "", False, 100000000, True)
                s = buildURL() & counter.ToString
            Catch
                retrycounter += 1
                GoTo tryagain
            End Try

            oRead = oFile.OpenText("WineList.txt")

            While oRead.Peek <> -1
                filetext += oRead.ReadLine() + vbNewLine
            End While

            oRead.Close()

            If filetext.Contains("<entry>") = False Then
                Exit While
            End If

            Try
                filetext = filetext.Remove(0, filetext.IndexOf("<entry>"))
                pagetolistxml()
            Catch
            End Try

            counter += 1
            System.IO.File.Delete("WineList.txt")
            ' loadWebpage(buildURL())
        End While

        If ListBox1.Items.Count = 0 Then
            ListBox1.Items.Add("No results returned")
        End If


        btnSearch.Enabled = True
        lblStatus.Text = "Status: Idle"
        lblReturned.Text = "Tweets returned:" & ListBox1.Items.Count
    End Sub

    Private Function buildURL()
        '"http://search.twitter.com/search.atom?q=from%3Arosevibe&until=2009-11-10&since=2009-11-10"
        Dim url As String = "http://search.twitter.com/search.atom?q=from%3A"
        url += edtUsername.Text
        url += "&until=" & DateTimePicker1.Value.ToString("yyyy-MM-dd")
        url += "&since=" & DateTimePicker1.Value.ToString("yyyy-MM-dd")
        url += "&page="
        Return url
    End Function

    Private Sub htmlButton()
        Dim oFile As System.IO.File
        Dim oRead As System.IO.StreamReader
        Dim counter As Integer
        Dim found As Boolean
        Dim retrycounter As Integer = 0

        counter = 1
        found = False
        ListBox1.Items.Clear()

        Dim myDTFI As DateTimeFormatInfo = New DateTimeFormatInfo()
        myDTFI.ShortDatePattern = "h:mm tt MMM d/c/c"


        If edtUsername.Text.Trim = "" Then
            MsgBox("A username must be entered.")
            Exit Sub
        End If

        While (True)

            filetext = ""

            Try
tryagain:
                If retrycounter = 5 Then
                    btnSearch.Enabled = True
                    lblStatus.Text = "Status: Connection error"
                    Exit Sub
                End If
                My.Computer.Network.DownloadFile("http://twitter.com/account/profile.mobile?page=" & counter.ToString & "&user=" & edtUsername.Text, "WineList.txt", "", "", False, 100000000, True)
            Catch
                retrycounter += 1
                GoTo tryagain
            End Try

            oRead = oFile.OpenText("WineList.txt")

            While oRead.Peek <> -1
                filetext += oRead.ReadLine() + vbNewLine
            End While

            oRead.Close()

            filetext = filetext.Remove(0, filetext.IndexOf("<div class=" & Chr(34) & "s" & Chr(34) & "><b>Previous Tweets</b></div>") + 62)
            Try
                filetext = filetext.Substring(0, filetext.IndexOf("</ul>"))
            Catch
                Exit While
            End Try

            If Not (filetext.Substring(filetext.IndexOf("<small>") + Len("<small>"), filetext.IndexOf("</small>") - filetext.IndexOf("<small>") - Len("<small>")).Contains("ago")) Then
                If DateTime.Parse(filetext.Substring(filetext.IndexOf("<small>") + Len("<small>"), filetext.IndexOf("</small>") - filetext.IndexOf("<small>") - Len("<small>")).Replace("rd", "").Replace("th", "").Replace("st", "").Replace("nd", ""), myDTFI) < DateTimePicker1.Value.Date Then
                    Exit While
                Else
                    lblStatus.Text = "Status: Searching... (" & DateTime.Parse(filetext.Substring(filetext.IndexOf("<small>") + Len("<small>"), filetext.IndexOf("</small>") - filetext.IndexOf("<small>") - Len("<small>")).Replace("rd", "").Replace("th", "").Replace("st", "").Replace("nd", ""), myDTFI).ToString("MMMM") & ") Tweets scanned: " & counter * 20
                End If
            End If

            If filetext.Contains(DateTimePicker1.Value.ToString("MMM d")) Then
                If (DateTimePicker1.Value.Year < Today.Year And filetext.Contains(DateTimePicker1.Value.Year.ToString & "</small>")) Or DateTimePicker1.Value.Year = Now().Year Then
                    found = True
                    pagetolisthtml()
                End If
            End If

            counter = counter + 1

            If found = True And (filetext.Contains(DateTimePicker1.Value.ToString("MMM d"))) = False Then
                Exit While
            End If

        End While

        If ListBox1.Items.Count = 0 Then
            ListBox1.Items.Add("No results returned")
        End If

        btnSearch.Enabled = True
        lblStatus.Text = "Status: Idle"
        System.IO.File.Delete("WineList.txt")

        lblReturned.Text = "Tweets returned:" & ListBox1.Items.Count



    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If DateTimePicker1.Value.Date > Today Then
            MsgBox("Search cannot be in the future")
            Exit Sub
        End If

        If edtUsername.Text.Trim = "" Then
            MsgBox("A username must be entered.")
            Exit Sub
        End If


        If DateDiff(DateInterval.Day, DateTimePicker1.Value.Date, Today) > 7 Then
            lblStatus.Text = "Status: Searching..."
            btnSearch.Enabled = False
            Dim trd As Thread = New Thread(AddressOf htmlButton)
            trd.IsBackground = True
            trd.Start()
            'htmlButton()
        Else
            lblStatus.Text = "Status: Searching..."
            btnSearch.Enabled = False
            Dim trd As Thread = New Thread(AddressOf twitterAPIButton)
            trd.IsBackground = True
            trd.Start()
            'twitterAPIButton()
        End If

        edtPreview.Text = ""
        ListBox1.SelectedIndex = -1
    End Sub


    Private Sub pagetolisthtml()
        Dim templist As String
        Dim tempstr As String

        tempstr = filetext

        While tempstr.Contains("<li>")
            tempstr = tempstr.Trim()
            templist = tempstr.Substring(4, tempstr.IndexOf("</li>") - 4)
            If templist.Contains(DateTimePicker1.Value.ToString("MMM d")) Then
                templist = fixlistitem(templist)
                ListBox1.Items.Add(templist)
            End If
            tempstr = tempstr.Remove(0, tempstr.IndexOf("</li>") + 5)
        End While
    End Sub

    Private Sub pagetolistxml()
        Dim templist As String
        Dim tempstr As String

        tempstr = filetext



        While tempstr.Contains("<entry>")
            tempstr = tempstr.Trim()
            templist = tempstr.Substring((tempstr.IndexOf("<title>") + Len("<title>")), tempstr.IndexOf("</title>") - (tempstr.IndexOf("<title>") + Len("<title>")))

            ListBox1.Items.Add(templist)
            tempstr = tempstr.Remove(0, tempstr.IndexOf("</entry>") + Len("</entry>"))
        End While
    End Sub


    Private Function fixlistitem(ByVal item As String)
        item = item.Substring(0, item.IndexOf("<small>"))

        While item.IndexOf("<") > -1 And item.IndexOf(">") > -1
            item = item.Remove(item.IndexOf("<"), item.IndexOf(">") - item.IndexOf("<") + 1)
        End While

        Return item
    End Function

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        edtPreview.Text = ListBox1.SelectedItem
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Control.CheckForIllegalCrossThreadCalls = False
    End Sub


End Class
