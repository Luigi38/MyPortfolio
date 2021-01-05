Imports System.IO
Imports System.Text
Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports NAudio.Wave.SampleProviders

''' <summary>
''' Invoke가 필요 없는 mciFunctions 클래스보다 향상된 사운드 관리자 입니다. mciFunctions랑 기능은 같지만 더 효율적인 코드를 제공합니다.
''' 또한 오디오 32비트를 지원합니다.
''' </summary>
Public Class SoundManager

    'SoundManager Reference.txt를 읽어주시면 SoundManager 함수를 이해하는데 도움이 됩니다.
    '현재 SoundManager 클래스가 쓰고 있는 사운드 API는 WasAPI 입니다.

    ''' <summary>
    ''' 사운드의 볼륨을 설정합니다. (기본 1.0)
    ''' </summary>
    Public Shared Property SoundVolume As Single = 1.0F
    ''' <summary>
    ''' 사운드의 지연 시간을 설정합니다. (기본 10ms)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property SoundLatency As Integer = SettingsManager.GetSoundLatencyValue()

    Private Shared enumerator As MMDeviceEnumerator = New MMDeviceEnumerator()
    Private Shared PlaybackDevice As String = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID

    Private Shared WaveOut As WasapiOut = New WasapiOut(enumerator.GetDevice(PlaybackDevice), AudioClientShareMode.Shared, True, SoundLatency)
    Private Shared Mixer As New MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) With {
        .ReadFully = True}
    Private Shared ctrl As New Dictionary(Of String, SoundLibrary)

    ''' <summary>
    ''' 소리 파일을 추가합니다. NAudio.Wave 네임스페이스를 사용하며 RAM (Random Access Memory)에 접근해서 소리를 빠르게 재생합니다.
    ''' 캐시 파일이기 때문에 속도가 굉장히 빠릅니다.
    ''' </summary>
    ''' <param name="chain"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="path"></param>
    Public Shared Sub AddSound(ByVal chain As Integer, ByVal x As String, ByVal y As Integer, ByVal index As Integer, ByVal path As String)

        Dim key As StringBuilder = New StringBuilder(100)

        If File.Exists(path) = False Then
            Throw New FileNotFoundException()
        End If

        Dim newSound As CachedSound = New CachedSound(path)

        key.Append(chain)
        key.Append(" ")
        key.Append(x)
        key.Append(" ")
        key.Append(y)
        key.Append(" ")
        key.Append(index)

        If ctrl.ContainsKey(key.ToString()) Then
            CloseSound(chain, x, y, index)
        End If

        Dim soundOnDisk As New WaveFileReader(path)
        ctrl.Add(key.ToString(), New SoundLibrary(soundOnDisk.TotalTime, newSound))

        soundOnDisk.Dispose()
        soundOnDisk = Nothing
    End Sub

    ''' <summary>
    ''' 사운드 플레이시 첫 초기화를 해주므로서 첫 SoundManager.PlaySound() 지연 시간을 단축해줍니다.
    ''' </summary>
    Public Shared Sub InitializeSound()
        WaveOut.Init(Mixer)
        WaveOut.Play()
    End Sub

    ''' <summary>
    ''' WasApi에서 사용하는 오디오 디바이스를 변경해줍니다.
    ''' </summary>
    Private Shared Sub ChangePlaybackDevice()
        PlaybackDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID
        WaveOut = New WasapiOut(enumerator.GetDevice(PlaybackDevice), AudioClientShareMode.Shared, True, SoundLatency)
        Mixer = New MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) With {
        .ReadFully = True}

        InitializeSound()
    End Sub

    ''' <summary>
    ''' 소리 파일을 재생합니다.
    ''' </summary>
    ''' <param name="sound">소리 재생 요청용 특정한 변수를 데이터로 보냅니다.</param>
    Public Shared Sub PlaySound(ByVal sound As SoundPlayEvent)
        If enumerator.GetDevice(PlaybackDevice).ID = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID Then

            If ctrl.ContainsKey(sound.Name) Then
                If sound.LoopNumber = 1 Then

                    'Sound Data
                    Dim cacheSoundSampleProvider As New CachedSoundSampleProvider(ctrl(sound.Name).soundOnMemory)

                    'Extension
                    Dim VolumeSampleProvider As New VolumeSampleProvider(cacheSoundSampleProvider) With {
                        .Volume = sound.SoundVolume}

                    'All mixer inputs must have the same WaveFormat
                    Dim resampler As New WdlResamplingSampleProvider(VolumeSampleProvider, 44100)
                    Dim ChannelResampler As ISampleProvider = resampler.ToStereo()

                    Mixer.AddMixerInput(ChannelResampler)

                    If Not WaveOut.PlaybackState = PlaybackState.Playing Then
                        WaveOut.Init(Mixer)
                        WaveOut.Play()
                    End If

#Region "뭔가 보기 싫은 이상한 코드"
                ElseIf sound.LoopNumber = 0 Then
                Else
#End Region
                End If
            End If
        Else
            ChangePlaybackDevice()
            PlaySound(sound)
        End If
    End Sub

    Public Shared Sub CloseSound(ByVal chain As Integer, ByVal x As String, ByVal y As Integer, ByVal index As Integer)
        Dim soundName As New StringBuilder(100)
        soundName.Append(chain)
        soundName.Append(" ")
        soundName.Append(x)
        soundName.Append(" ")
        soundName.Append(y)
        soundName.Append(" ")
        soundName.Append(index)

        If ctrl.ContainsKey(soundName.ToString()) Then
            ctrl(soundName.ToString()) = Nothing
            ctrl.Remove(soundName.ToString())
        End If
    End Sub

    Public Shared Sub CloseAllSounds()
        Dim keys As String() = ctrl.Keys.ToArray()

        For i As Integer = 0 To keys.Count - 1
            ctrl.Remove(keys(i))
        Next
    End Sub

    Public Shared Sub Close()
        CloseAllSounds()
        enumerator.Dispose()
        WaveOut.Stop()
        WaveOut.Dispose()
    End Sub

    ''' <summary>
    ''' 추가한 사운드의 key로 통해 사운드의 길이를 구합니다.
    ''' </summary>
    Public Shared Function GetSoundLength(ByVal chain As Integer, ByVal x As String, ByVal y As Integer, ByVal index As Integer) As TimeSpan
        Dim soundName As New StringBuilder(100)
        soundName.Append(chain)
        soundName.Append(" ")
        soundName.Append(x)
        soundName.Append(" ")
        soundName.Append(y)
        soundName.Append(" ")
        soundName.Append(index)


        If ctrl.ContainsKey(soundName.ToString()) = False Then
            Return TimeSpan.FromMilliseconds(0)
        End If

        Return ctrl(soundName.ToString()).soundTime
    End Function

    ''' <summary>
    ''' 로컬 파일로 접근하여 사운드의 길이를 구합니다.
    ''' </summary>
    ''' <param name="path">로컬 파일의 위치</param>
    ''' <returns></returns>
    Public Shared Function GetSoundLength(ByVal path As String) As TimeSpan
        If File.Exists(path) = False Then
            Return TimeSpan.FromMilliseconds(0)
        End If

        Dim sound As AudioFileReader = New AudioFileReader(path)
        Dim soundLength As TimeSpan = sound.TotalTime

        sound.Dispose()
        sound = Nothing
        Return soundLength
    End Function

    ''' <summary>
    ''' 현재 사운드 플레이어가 플레이 중인지 물어봅니다. 읽기 전용입니다.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property IsPlaying As Boolean
        Get
            Return WaveOut.PlaybackState = PlaybackState.Playing
        End Get
    End Property

    Private Structure SoundLibrary
        Public soundTime As TimeSpan
        Public soundOnMemory As CachedSound

        Sub New(soundTime As TimeSpan, soundOnMemory As CachedSound)
            Me.soundTime = soundTime
            Me.soundOnMemory = soundOnMemory
        End Sub
    End Structure
End Class

#Region "CachedSound Class"
Friend Class CachedSound
    Private _AudioData As Single(), _WaveFormat As WaveFormat

    Public Property AudioData As Single()
        Get
            Return _AudioData
        End Get
        Private Set(ByVal value As Single())
            _AudioData = value
        End Set
    End Property

    Public Property WaveFormat As WaveFormat
        Get
            Return _WaveFormat
        End Get
        Private Set(ByVal value As WaveFormat)
            _WaveFormat = value
        End Set
    End Property

    Public Sub New(ByVal audioFileName As String)
        Using audioFileReader = New AudioFileReader(audioFileName)
            WaveFormat = audioFileReader.WaveFormat
            Dim wholeFile = New List(Of Single)(CInt(audioFileReader.Length / 4))
            Dim readBuffer = New Single(audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels - 1) {}
            Dim samplesRead As Integer = audioFileReader.Read(readBuffer, 0, readBuffer.Length)

            While samplesRead > 0
                wholeFile.AddRange(readBuffer.Take(samplesRead))
                samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)
            End While

            AudioData = wholeFile.ToArray()
        End Using
    End Sub
End Class

Friend Class CachedSoundSampleProvider
    Implements ISampleProvider

    Private ReadOnly cachedSound As CachedSound
    Private position As Long

    Public Sub New(ByVal cachedSound As CachedSound)
        Me.cachedSound = cachedSound
    End Sub

    Public Function Read(buffer() As Single, offset As Integer, count As Integer) As Integer Implements ISampleProvider.Read
        Dim availableSamples = cachedSound.AudioData.Length - position
        Dim samplesToCopy = Math.Min(availableSamples, count)
        Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy)
        position += samplesToCopy
        Return CInt(samplesToCopy)
    End Function

    Public ReadOnly Property WaveFormat As WaveFormat Implements ISampleProvider.WaveFormat
        Get
            Return cachedSound.WaveFormat
        End Get
    End Property
End Class

#End Region