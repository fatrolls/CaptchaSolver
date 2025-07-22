Imports System.Net
Imports System.IO
Imports System.Drawing
Imports System.ComponentModel


Public Class PixelValues
    Public PixelValue As Double = 0
    Public Black As Boolean = False
End Class

Public Class ColoredRectangle

    Private myPosition As Point = New Point(0, 0)
    Private mySize As Size = New Size(10, 10)
    Private myColoredPixels As Integer = 0

    Public Sub New(ByVal Position As Point)
        myPosition = Position
    End Sub

    Public Sub New(ByVal Position As Point, ByVal Size As Size)
        myPosition = Position
        mySize = Size
    End Sub

    Public ReadOnly Property Size() As Size
        Get
            Return mySize
        End Get
    End Property

    Public ReadOnly Property Position() As Point
        Get
            Return myPosition
        End Get
    End Property

    Public Sub New(ByVal X As Integer, ByVal Y As Integer, ByVal Width As Integer, ByVal Height As Integer)
        myPosition = New Point(X, Y)
        mySize = New Size(Width, Height)
    End Sub

    Public ReadOnly Property ColoredPixels() As Integer
        Get
            Return myColoredPixels
        End Get
    End Property

    Public Sub AddPixel()
        myColoredPixels += 1
    End Sub

End Class


Public Class classCaptcha

    Private Coordinates(,) As PixelValues

    Public ReadOnly CaptchaPath As String = "http://www.pennergame.de/security/captcha12223213.jpg"

    Public ReadOnly MaxColorIntValue As Integer = captchaForm.numGreyValue.Value
    Public ReadOnly MaxColorValue As Color = Color.FromArgb(MaxColorIntValue, MaxColorIntValue, MaxColorIntValue)

    Private WithEvents myDownloadBG As New BackgroundWorker

    Private myImageStream As MemoryStream = Nothing

    Private myOriginalCaptcha As Image = Nothing
    Public ReadOnly Property OriginalCaptcha() As Image
        Get
            Return myOriginalCaptcha
        End Get
    End Property

    Private myBlackWhiteCaptcha As Bitmap = Nothing
    Public ReadOnly Property BlackWhiteCaptcha() As Bitmap
        Get
            Return myBlackWhiteCaptcha
        End Get
    End Property

    Private myWeightedWidthCaptcha As Bitmap = Nothing
    Public ReadOnly Property WeightedWidthCaptcha() As Bitmap
        Get
            Return myWeightedWidthCaptcha
        End Get
    End Property

    Private myMarkedCaptcha As Bitmap = Nothing
    Public ReadOnly Property MarkedCaptcha() As Bitmap
        Get
            Return myMarkedCaptcha
        End Get
    End Property

    Private myWeightHeightCaptcha As Bitmap = Nothing
    Public ReadOnly Property WeightedHeightCaptcha() As Bitmap
        Get
            Return myWeightHeightCaptcha
        End Get
    End Property

    Private myTresholdCaptcha As Bitmap = Nothing
    Public ReadOnly Property TresholdCaptcha() As Bitmap
        Get
            Return myTresholdCaptcha
        End Get
    End Property

    Private myNeuronalCaptcha As Bitmap = Nothing
    Public ReadOnly Property NeuronalCaptcha() As Bitmap
        Get
            Return myNeuronalCaptcha
        End Get
    End Property

    Private myHeightPoints As List(Of Integer)
    Public ReadOnly Property HeightPoints() As List(Of Integer)
        Get
            Return myHeightPoints
        End Get
    End Property

    Private myWidthPoints As List(Of Integer)
    Public ReadOnly Property WidthPoints() As List(Of Integer)
        Get
            Return myWidthPoints
        End Get
    End Property

    Public Sub New()
        myDownloadBG.RunWorkerAsync()
    End Sub

    Private Sub DownloadPic() Handles myDownloadBG.DoWork
        Dim myImageWC As New WebClient
        myImageStream = New MemoryStream(myImageWC.DownloadData(CaptchaPath))
    End Sub

    Private Sub DownloadPicComplete() Handles myDownloadBG.RunWorkerCompleted
        myOriginalCaptcha = Image.FromStream(myImageStream)
        captchaForm.picOriginalCaptcha.Image = myOriginalCaptcha
    End Sub

    Public Sub BlackAndWhite()
        Dim ImgSize As Size = myOriginalCaptcha.Size
        ReDim Coordinates(ImgSize.Width - 1, ImgSize.Height - 1)
        For X As Integer = 0 To ImgSize.Width - 1
            For Y As Integer = 0 To ImgSize.Height - 1
                Coordinates(X, Y) = New PixelValues
            Next
        Next
        Dim CaptchaBitmap As Bitmap = myOriginalCaptcha.Clone
        For X As Integer = 0 To ImgSize.Width - 1
            For Y As Integer = 0 To ImgSize.Height - 1
                Dim OriginalColor As Color = CaptchaBitmap.GetPixel(X, Y)
                If Not CheckPixel(OriginalColor) Then
                    CaptchaBitmap.SetPixel(X, Y, Color.FromArgb(255, 255, 255))
                Else
                    CaptchaBitmap.SetPixel(X, Y, Color.FromArgb(0, 0, 0))
                    Coordinates(X, Y).Black = True
                End If
            Next
        Next
        Dim gfx As Graphics = Graphics.FromImage(CaptchaBitmap)
        gfx.FillRectangle(Brushes.White, 0, 40, 205, 54)
        myBlackWhiteCaptcha = CaptchaBitmap
        For Y As Integer = 0 To ImgSize.Height - 1
            Dim Text As String = ""
            For X As Integer = 0 To ImgSize.Width - 1

                If Coordinates(X, Y).Black = True Then
                    Text = Text & "X"
                Else
                    Text = Text & " "
                End If
            Next
            Debug.Print(Text)
        Next

    End Sub

    Public Sub WeightInWidth()
        Dim WeightWidthBitmap As New Bitmap(myOriginalCaptcha.Width, myBlackWhiteCaptcha.Height)
        myHeightPoints = New List(Of Integer)
        myWidthPoints = New List(Of Integer)
        For X As Integer = 0 To WeightWidthBitmap.Width - 1
            Dim Weight As Integer = 0
            For Y As Integer = 0 To myBlackWhiteCaptcha.Height - 1
                Dim OriginalColor As Color = myBlackWhiteCaptcha.GetPixel(X, Y)
                If CheckPixel(OriginalColor) Then
                    Weight += 1
                End If
            Next
            HeightPoints.Add(Weight)
            For Y As Integer = 0 To Weight - 1
                WeightWidthBitmap.SetPixel(X, Y, Color.Black)
            Next
        Next
        Dim HeightAverage As Double = HeightPoints.Average()
        For I As Integer = 0 To HeightPoints.Count - 1
            If HeightPoints(I) < HeightAverage Then
                WeightWidthBitmap.SetPixel(I, 0, Color.Black)
            End If
        Next
        myWeightedWidthCaptcha = WeightWidthBitmap
        Dim WeightHeightBitmap As New Bitmap(myOriginalCaptcha.Width, myBlackWhiteCaptcha.Height)
        For Y As Integer = 0 To WeightHeightBitmap.Height - 1
            Dim Weight As Integer = 0
            For X As Integer = 0 To myBlackWhiteCaptcha.Width - 1
                Dim OriginalColor As Color = myBlackWhiteCaptcha.GetPixel(X, Y)
                If CheckPixel(OriginalColor) Then
                    Weight += 1
                End If
            Next
            WidthPoints.Add(Weight)
            For X As Integer = 0 To Weight - 1 '(WeightHeightBitmap.Height / Weight) * 100
                WeightHeightBitmap.SetPixel(X, Y, Color.Black)
            Next
        Next
        Dim WidthAverage As Double = WidthPoints.Average()
        For I As Integer = 0 To WidthPoints.Count - 1
            If WidthPoints(I) < WidthAverage Then
                WeightHeightBitmap.SetPixel(0, I, Color.Black)
            End If
        Next
        myWeightHeightCaptcha = WeightHeightBitmap
    End Sub

    Public Sub markAverage()
        Dim AverageMarked As Bitmap = BlackWhiteCaptcha.Clone
        Dim Gfx As Graphics = Graphics.FromImage(AverageMarked)
        'For Y As Integer = 0 To WidthPoints.Count - 1
        '    If WidthPoints(Y) > WidthPoints.Average Then
        '        Gfx.DrawLine(Pens.Red, 0, Y, AverageMarked.Width, Y)
        '    End If
        'Next
        For X As Integer = 0 To HeightPoints.Count - 1
            For Y As Integer = 0 To WidthPoints.Count - 1
                If HeightPoints(X) < (HeightPoints.Average) And WidthPoints(Y) < (WidthPoints.Average) Then
                    Dim PixelColor As Color = AverageMarked.GetPixel(X, Y)
                    If CheckPixel(PixelColor) Then
                        AverageMarked.SetPixel(X, Y, Color.Black)
                    Else
                        AverageMarked.SetPixel(X, Y, Color.White)
                    End If
                    'Gfx.DrawLine(Pens.Red, X, 0, X, AverageMarked.Height)

                Else
                    AverageMarked.SetPixel(X, Y, Color.White)
                End If
            Next
        Next
        myMarkedCaptcha = AverageMarked
    End Sub

    Public Sub MarkTreshold()
        Dim TresHoldBitmap As Bitmap = BlackWhiteCaptcha.Clone
        If Not NeuronalCaptcha Is Nothing Then
            TresHoldBitmap = NeuronalCaptcha.Clone
        End If
        Dim GFX As Graphics = Graphics.FromImage(TresHoldBitmap)
        For ThresholdNumber As Integer = captchaForm.numTreshold.Value To captchaForm.numTresholdMax.Value
            Dim TresHoldMax As Integer = ThresholdNumber * ThresholdNumber
            For Y As Integer = 0 To TresHoldBitmap.Height - 1 Step ThresholdNumber
                For X As Integer = 0 To TresHoldBitmap.Width - 1 Step ThresholdNumber
                    Dim PixCount As Integer = 0
                    For I As Integer = 0 To ThresholdNumber
                        For j As Integer = 0 To ThresholdNumber
                            If Y + j >= TresHoldBitmap.Height Then Continue For
                            If X + I >= TresHoldBitmap.Width Then Continue For
                            Dim PixelColor As Color = myBlackWhiteCaptcha.GetPixel(X + I, Y + j)
                            If CheckPixel(PixelColor) Then
                                PixCount += 1

                            End If
                        Next
                    Next
                    If PixCount > (TresHoldMax * captchaForm.numMinimum.Value) Then
                        GFX.FillRectangle(Brushes.Black, X, Y, ThresholdNumber, ThresholdNumber)
                    Else
                        GFX.FillRectangle(Brushes.White, X, Y, ThresholdNumber, ThresholdNumber)
                    End If
                    Coordinates(X, Y).PixelValue = PixCount
                Next
            Next

            myTresholdCaptcha = TresHoldBitmap
        Next

    End Sub

    Dim PixelSquareSize As Integer = 3

    Private Function CheckPixel(ByVal Pixel As Color) As Boolean
        If Pixel.R < MaxColorValue.R AndAlso Pixel.G < MaxColorValue.G AndAlso Pixel.B < MaxColorValue.B Then
            Return True
        Else
            Return False
        End If
    End Function


    Public Sub NeuronalSearch()
        If BlackWhiteCaptcha Is Nothing Then
            Me.BlackAndWhite()
        End If
        If HeightPoints Is Nothing Then
            Me.WeightInWidth()
        End If
        Dim myPixelSquaredImage As Bitmap = BlackWhiteCaptcha.Clone
        'If Not myNeuronalCaptcha Is Nothing Then
        '    myPixelSquaredImage = myNeuronalCaptcha.Clone
        'End If
        Dim TotalPixelValue As Double = 0
        Dim Treshold As Double = captchaForm.numNeuralTreshold.Value
        Dim OwnBlack As Double = captchaForm.numBlackPixel.Value
        Dim DirectNeighbour As Double = captchaForm.numDirectNeighbour.Value
        Dim DiagonalNeighbour As Double = captchaForm.numDiagonalNeighbour.Value
        For X As Integer = 0 To myPixelSquaredImage.Width - 1
            For Y As Integer = 0 To myPixelSquaredImage.Height - 1
                If HeightPoints(X) > HeightPoints.Average AndAlso WidthPoints(Y) > WidthPoints.Average Then
                    TotalPixelValue = 0
                    Dim Pixel As Color = myPixelSquaredImage.GetPixel(X, Y)
                    If CheckPixel(Pixel) Then
                        TotalPixelValue += OwnBlack
                        '                    Debug.Print(String.Format("Ausgewählt ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                    End If
                    If X - 1 > 0 Then
                        Dim LeftPixel As Color = myPixelSquaredImage.GetPixel(X - 1, Y)
                        If CheckPixel(LeftPixel) Then
                            TotalPixelValue += DirectNeighbour
                            '                       Debug.Print(String.Format("Links ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If X + 1 < myPixelSquaredImage.Width - 1 Then
                        Dim RightPixel As Color = myPixelSquaredImage.GetPixel(X + 1, Y)
                        If CheckPixel(RightPixel) Then
                            TotalPixelValue += DirectNeighbour
                            '                        Debug.Print(String.Format("Rechts ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If Y - 1 > 0 Then
                        Dim TopPixel As Color = myPixelSquaredImage.GetPixel(X, Y - 1)
                        If CheckPixel(TopPixel) Then
                            TotalPixelValue += DirectNeighbour
                            '                        Debug.Print(String.Format("Oben ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If Y + 1 < myPixelSquaredImage.Height - 1 Then
                        Dim BottomPixel As Color = myPixelSquaredImage.GetPixel(X, Y + 1)
                        If CheckPixel(BottomPixel) Then
                            TotalPixelValue += DirectNeighbour
                            '                        Debug.Print(String.Format("Unten ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If X - 1 > 0 AndAlso Y - 1 > 0 Then
                        Dim LeftTopPixel As Color = myPixelSquaredImage.GetPixel(X - 1, Y - 1)
                        If CheckPixel(LeftTopPixel) Then
                            TotalPixelValue += DiagonalNeighbour
                            '                        Debug.Print(String.Format("Links Oben ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If X + 1 < myPixelSquaredImage.Width - 1 AndAlso Y - 1 > 0 Then
                        Dim LeftBottomPixel As Color = myPixelSquaredImage.GetPixel(X + 1, Y - 1)
                        If CheckPixel(LeftBottomPixel) Then
                            TotalPixelValue += DiagonalNeighbour
                            '                        Debug.Print(String.Format("Links unten ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If X - 1 > 0 AndAlso Y + 1 < myPixelSquaredImage.Height - 1 Then
                        Dim RightTopPixel As Color = myPixelSquaredImage.GetPixel(X - 1, Y + 1)
                        If CheckPixel(RightTopPixel) Then
                            TotalPixelValue += DiagonalNeighbour
                            '                        Debug.Print(String.Format("Rechts oben ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    If X + 1 < myPixelSquaredImage.Width - 1 AndAlso Y + 1 < myPixelSquaredImage.Height - 1 Then
                        Dim RightBottomPixel As Color = myPixelSquaredImage.GetPixel(X + 1, Y + 1)
                        If CheckPixel(RightBottomPixel) Then
                            TotalPixelValue += DiagonalNeighbour
                            '                        Debug.Print(String.Format("Rechts unten ist Schwarz (Wert: {0:N1})", TotalPixelValue))
                        End If
                    End If
                    '                Debug.Print(String.Format("Pixel an Position X: {0}, Y: {1} hat den Wert {2:N1}", X, Y, TotalPixelValue))
                    If TotalPixelValue >= Treshold Then
                        myPixelSquaredImage.SetPixel(X, Y, Color.Black)
                    Else
                        myPixelSquaredImage.SetPixel(X, Y, Color.White)
                    End If
                End If
            Next
        Next
        myNeuronalCaptcha = myPixelSquaredImage
    End Sub


    Public Sub PixelCountMethod()
        Dim RectSize As New Size(22, 34)
        Dim MyImage As Bitmap = NeuronalCaptcha.Clone
        Dim CheckedRectancles As New List(Of ColoredRectangle)
        Dim MaxFoundPixels As Integer = 0
        Dim MaxFoundRectangle As ColoredRectangle = Nothing
        For X As Integer = 0 To (MyImage.Width - 1 - RectSize.Width) Step 2
            For Y As Integer = 0 To (MyImage.Height - 1 - RectSize.Height) Step 2
                Dim CurrentRectangle As New ColoredRectangle(X, Y, RectSize.Width, RectSize.Height)
                For I As Integer = 0 To RectSize.Width - 1
                    For J As Integer = 0 To RectSize.Height
                        If CheckPixel(MyImage.GetPixel(X + I, J + I)) Then
                            CurrentRectangle.AddPixel()
                        End If
                    Next
                Next
                If CurrentRectangle.ColoredPixels > MaxFoundPixels Then
                    MaxFoundPixels = CurrentRectangle.ColoredPixels
                    MaxFoundRectangle = CurrentRectangle
                End If
                'CheckedRectancles.Add(CurrentRectangle)
            Next
        Next
        Dim GFX As Graphics = Graphics.FromImage(OriginalCaptcha)
        With MaxFoundRectangle
            GFX.DrawRectangle(Pens.Red, .Position.X, .Position.Y, .Size.Width, .Size.Height)
        End With
        captchaForm.EndImage.Image = OriginalCaptcha
    End Sub


End Class
