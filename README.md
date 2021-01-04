# 나의 포트폴리오

## 나에 대해
2006년에 태어나 초등학교 2학년 때 프로그래밍을 알고 공부하게 되었음.

첫 프로젝트는 초등학교 6학년 때부터 시작하여 계속 이어져 오고 있음. 또한 여러 프로그래밍 언어들을 가지고 수준 높은 코드를 구상하고 짜게 됨.
나의 장점은 한번 프로젝트를 만들기 시작하면 포기하지 않고 최선을 다하는 것임.

## 프로젝트
### 유니터
유니터는 유니패드(런치패드를 스마트폰에서도 쓸 수 있게 만들어주는 모바일 애플리케이션) 관련 IDE 프로그램.
Visual Basic .NET으로 만들어짐.

**INI 파일에서 Dictionary (Map)으로 변환하는 알고리즘**
```vb.net
Public Shared Function GetIniValue(section As String, key As String, filename As String, Optional defaultValue As String = "") As String
    Dim dict As Dictionary(Of String, String) = GetSection(section, filename)

    If dict.Count = 0 OrElse dict.ContainsKey(key) = False Then
        Return defaultValue
    End If

    Return dict(key)
End Function

Private Shared Function GetSection(section As String, filename As String) As Dictionary(Of String, String)
    Dim lines As String() = File.ReadAllLines(filename, Encoding.UTF8) 'FILE MUST ENCODING IS UTF8
    Dim write_section As Boolean = False
    Dim result As New Dictionary(Of String, String)

    For i As Integer = 0 To lines.Length - 1
        If lines(i).StartsWith(";") Then
            Continue For
        ElseIf lines(i).StartsWith("[") Then
            If String.Compare(lines(i), "[" & section & "]") = 0 Then
                write_section = True
            Else
                write_section = False
            End If
        ElseIf String.IsNullOrWhiteSpace(lines(i)) Then
            Continue For
        ElseIf write_section = False Then
            Continue For
        End If

        Dim sp As String() = lines(i).Split("=")

        If sp.Length >= 2 Then
            result.Add(sp(0), sp(1))
        End If
    Next

    Return result
End Function
```
이미 ini 파일을 읽는 함수는 있지만, UTF-8을 지원하지 않아서 직접 만듦.
테스트 결과, 잘 출력됨. 다만 파일이 UTF-8으로만 인코딩이 되어야 함.

**소리 재생**
```vb.net
''' <summary>
''' 소리 파일을 재생합니다.
''' </summary>
Public Shared Sub PlaySound(name As String, Optional soundVolume As Single = 1.0)
    If enumerator.GetDevice(PlaybackDevice).ID = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID Then

        If ctrl.ContainsKey(name) Then
                'Sound Data
                Dim cacheSoundSampleProvider As New CachedSoundSampleProvider(ctrl(sound.Name).soundOnMemory)
                Dim VolumeSampleProvider As New VolumeSampleProvider(cacheSoundSampleProvider) With {
                    .Volume = soundVolume}

                Dim resampler As New WdlResamplingSampleProvider(VolumeSampleProvider, 44100)
                Dim ChannelResampler As ISampleProvider = resampler.ToStereo()

                Mixer.AddMixerInput(ChannelResampler)
        End If
    Else
        ChangePlaybackDevice()
        PlaySound(name, soundVolume)
    End If
End Sub

''' <summary>
''' 사운드 플레이시 첫 초기화를 해주므로서 첫 PlaySound() 지연 시간을 단축해줍니다.
''' </summary>
Public Shared Sub InitializeSound()
  Wasapi.Init(Mixer)
  Wasapi.Play()
End Sub
```
My.Computer.Audio.Play() 함수로도 충분히 소리 재생이 가능하지만, 소리가 다중으로 들리지 않고 겹치지 않으며, 약간의 지연 시간이 있음.

현재 프로젝트 (유니터)에서는 새로운 소리 재생 알고리즘이 필요하다고 생각함.
따라서 소리 재생 알고리즘을 찾아봄.

WaveOut, DirectSound, Wasapi, Asio 중에서 Wasapi를 고름.

- WaveOut: 지연 시간이 높으며, 몇몇 컴퓨터에서는 소리가 불안정하게 들림.
- DirectSound: 대부분의 컴퓨터에서는 지연 시간이 낮지만, 일부 컴퓨터에서는 지연 시간이 높음. 또한 소리가 불안정하게 들리는 문제점이 있음.
- Wasapi: 지연 시간이 낮으며, 소리가 안정하게 들림. 현재 프로젝트에서 가장 적합함.
- Asio: 지연 시간이 낮으며, 소리가 안정하게 들림. 하지만 드라이버를 새로 깔아야함.

[NAudio](https://github.com/naudio/NAudio) 라이브러리를 통해 소리 재생 알고리즘을 구현함.

- 또한 소리가 다중으로 들려야 하며 겹쳐야 하므로 Mixer를 이용함.

- 몇몇 소리는 파일 포맷이 일부 소리 파일과 같지 않을 경우 변환하여 오류가 나지 않도록 함.

- 지연 시간이 매우 낮아야 하므로 소리를 ctrl이라는 사전 (맵)에 캐시로 저장하고 소리를 재생할 때 저장되어 있던 캐시를 꺼내 지연 없이 소리를 아주 빠르게 재생 할 수 있음.

- 소리를 재생하던 도중 재생 장치가 바뀔 수 있으므로 이전에 들었던 재생 장치의 ID와 현재 소리를 재생하려고 하는 재생 장치의 ID를 비교하여 소리를 재생하던 도중 재생 장치가 바뀌어도 소리를 원활하게 재생할 수 있음.

- 또한 초반에 미리 데이터가 비어있는 소리를 재생시켜주므로서 소리를 재생시키까지의 지연 시간을 단축함.

### Send Anywhere 비공식 API
Send Anywhere (파일 전송 및 공유 애플리케이션)을 다른 프로그램에서도 사용할 수 있는 API.

비공식이다 보니 개발 초기 몇 주간은 Send Anywhere의 기능에 대해 연구함.
혼자 연구하다 보니 개발하기 너무 힘들었음. 하지만 시행착오를 통해 결국 기능을 구현하게 됨.

Python과 C#으로 만들어짐.

**파일을 서버에 전송**
```python
key = 0

def send(self):
  req_session_start = requests.get(session_start_link, json=files_payload)
  key_data = req_session_start.json()

  key = key_data['key']

  for x in key_data['file']:
      requests.post(data_upload_link + key, files={'file': file_data})

  requests.get(session_link)
  requests.get(session_finish_link)
```
```c#
private int key = 0;

private async Task<Dictionary<string, string>> SendAsync()
{
  HttpClient httpClient = new HttpClient();
  
  UriBuilder baseUri = new UriBuilder(session_start_link);

  HttpResponseMessage req = await httpClient.PostAsync(baseUri.Uri, payload);
  string json = await req.Content.ReadAsStringAsync();
  var key_data = JsonToDictionary<string, object>(json);

  foreach (var x in key_data["file"])
  {
      baseUri = new UriBuilder(data_upload_link + key.ToString());

      MultipartFormDataContent fileContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
      fileContent.Add(new StreamContent(new MemoryStream(file_data)), file_name, file_name);

      await httpClient.PostAsync(baseUri.Uri, fileContent);
  }

  //session end
  baseUri = new UriBuilder(session_link);
  await httpClient.GetAsync(baseUri.Uri);

  baseUri = new UriBuilder(session_finish_link);
  await httpClient.GetAsync(baseUri.Uri);
}
```
파일을 서버로 보내기까지의 함수 코드.
1. 파일을 보내겠다는 세션 시작 링크를 request함.
2. 세션 링크를 request하고 파일을 보냄.
3. 세션 닫는 링크를 request하여 성공적으로 서버에 파일을 보냄.

**서버에 있던 파일 받기**
```python
req = requests.get(link, headers=headers)  # download
file_name = req.headers['Content-Disposition'].split("filename=")[1].strip('"').encode("ISO-8859-1").decode("utf-8")
file_data = req.content

open(file_name, 'wb').write(file_data)
```
```c#
using (WebClient web = new WebClient())
{
    byte[] fileData = await web.DownloadDataTaskAsync(url);
    string fileName = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1")
        .GetBytes(web.ResponseHeaders["Content-Disposition"]
            .Split(new string[] {"filename="}, StringSplitOptions.None)[1].Trim('"')));

    File.WriteAllBytes(fileName, fileData);
}
```
request를 통해 서버에 있던 파일의 데이터와 이름을 가져옴.

### 폴록봇
디스코드 대화봇. 봇이랑 장난을 치며 대화를 할 수 있음.

Python으로 만들어짐.

**AES-256 암호화 및 복호화**
```python
def encrypt(x: str, key: str) -> str:
        raw = AES256.__pad(x)
        iv = Random.new().read(AES.block_size)
        cipher = AES.new(key.encode("utf-8"), AES.MODE_CBC, iv)
        return base64.b64encode(iv + cipher.encrypt(raw.encode('utf-8'))).decode('utf-8')

def decrypt(x: str, key: str) -> str:
    enc = base64.b64decode(x)
    iv = enc[:16]
    cipher = AES.new(key.encode("utf-8"), AES.MODE_CBC, iv)
    return AES256.__unpad(cipher.decrypt(enc[16:])).decode('utf-8')

def __pad(s: str) -> str:
    BS = 16
    return s + (BS - len(s.encode('utf-8')) % BS) * chr(BS - len(s.encode('utf-8')) % BS)

def __unpad(s: bytes) -> bytes:
    return s[:-ord(s[len(s) - 1:])]
    
str = "Hello, World!"
key = "HelloWorldThisIsTestSoVeryFunnyy"
str_encrypted = encrypt(str, key) # yC4iXAwy7E80U3KXcycfyw==
str_decrypted = decrypt(str_encrypted, key) # Hello, World! (same as str)
```
오류 메시지가 나올 때 평문으로 나올 경우 소스 코드가 유출이 될 위험이 있음.
- AES-256이랑 시크릿 키를 이용하여 오류 메시지 암호화.

- 개발자는 자신만 알고 있는 시크릿 키로 오류 메시지 복호화.

- 소스 코드 유출이 될 수 없음.

### Project A
SNS 프로그램.

C#으로 만들어짐.

**SHA-512 해시**
```c#
public static string Hash(string str, bool lowerCase = true)
{
    string fin = "";
    byte[] datas = Encoding.UTF8.GetBytes(str);
    
    using (System.Security.Cryptography.SHA512 sha = new System.Security.Cryptography.SHA512Managed())
    {
        byte[] datas_with_hash = sha.ComputeHash(datas);
        fin = BitConverter.ToString(datas_with_hash).Replace("-", "");
    }
    if (lowerCase) fin = fin.ToLower();
    
    return fin;
}
        
string password = "helloworldthisistest1234";
string password_hash = Hash(password); //b3ab4bb5f6a2a94b9810bd7e9002a92d57a29f9d5cb51f555a97213912d91adfa415f9f045054b32045d4609b339fdb6c4906fac6a50555c601fc785824c7ad4
```
로그인을 할 때 해시 함수 없이 그대로 평문으로 비밀번호를 전달하면 보안의 위험성이 있음.
- 따라서 SHA-512 해시 함수를 통해 비밀번호를 서버로 전달함.

## 언어
C, C++, C#, Visual Basic .NET, Python, Java, Kotlin, Go

## 개발 IDE 프로그램
Visual Studio, Visual Studio Code, PyCharm, Android Studio
