Imports System.Text
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text.RegularExpressions

Namespace Manage.Translation
    Public Class translateReader
        Public Shared lang_ As String = "English"

#Region "로딩 폼 텍스트"
        Public Shared loading_default_windowstring As String = "Loading..."
        Public Shared loading_initdir As String = "Initializing Workspace Directory..."
        Public Shared loading_unzipping As String = "Unzipping files..."
        Public Shared loading_keySoundLoading As String = "Loading Sounds... ({0}/{1})"
        Public Shared loading_keyLEDLoading As String = "Loading LEDs... ({0}/{1})"
        Public Shared loading_removeChain As String = "Removing Chain..."
        Public Shared loading_removeWorkspace As String = "Flushing temporary directory..."
        Public Shared loading_load_converting As String = "Converting MP3 file to WAVE file... {0}"
#End Region

#Region "MessageBox Texts"
        Public Shared msg_Main_TooManyChains_Text As String = "There are 24 chains already in this UniPack. You can't add more chain."
        Public Shared msg_Main_TooManyChains_Title As String = "Maximum Chain"
        Public Shared msg_Main_Load_infoRequirementsNotOK_Text As String = "This is UniPack is not supported! Unitor supports only 8 x 8 and squarebutton enabled and a unipack which has maximum 24 chains."
        Public Shared msg_Main_Load_infoRequirementsNotOK_Title As String = "UniPack Not Supported"
        Public Shared msg_Main_0Chain_Text As String = "No chains left! You can't delete a chain!"
        Public Shared msg_Main_0Chain_Title As String = "No chains left"

        Public Shared msg_Main_RemoveChain_Title As String = "Chain Removement Warning"
        Public Shared msg_Main_RemoveChain_Text As String = "Do you want to delete selected chain? This process cannot be undone!"

        Public Shared msg_Main_Load_HigherChain_Text As String = "Loading process stopped. Following sound is assigned to a chain higher than chain number in info file!" & vbNewLine & "Line Number: {0}"
        Public Shared msg_Main_Load_HigherChain_Title As String = "Bad Chain Number"

        Public Shared msg_soundedit_dragdropAlreadyExists_Text As String = "Selected sound '{0}' is already exists in UniPack sound library. Do you want to overwrite?"
        Public Shared msg_soundedit_dragdropAlreadyExists_Title As String = "Sound Already Exists"

        Public Shared msg_soundedit_onlywav_Text As String = "Only .wav file is supported. Unitor can't load this file. Please change extension to .wav => {0}" & vbNewLine & vbNewLine & "Tip: Unitor can convert mp3 file to wav file with 'Convert Mp3 files to Wav files automatically' in settings."
        Public Shared msg_soundedit_onlywav_Title As String = "Extension Mismatch"

        Public Shared msg_load_noinfo_Text As String = "No info file in this UniPack! Is this a correct UniPack? Unitor can't load this file."
        Public Shared msg_load_noinfo_Title As String = "No info file"
        Public Shared msg_load_nokeySound_Text As String = "No keySound file in this UniPack! Is this a correct UniPack? Unitor can't load this file."
        Public Shared msg_load_nokeySound_Title As String = "No keySound file"

        Public Shared msg_main_removekeyled_title As String = "Removing LED data"
        Public Shared msg_main_removekeyled_text As String = "Do you really want to remove ALL LED data? This process cannot be undone! (LED is still enabled but all scripts will be removed.)"

        Public Shared msg_main_removesounds_title As String = "Removing Sound Data"
        Public Shared msg_main_removesounds_text As String = "Do you want to remove ALL Sound data? This process cannot be undone!"

        Public Shared msg_soundedit_LoopNum_text As String = "Please input the loop number of the new sound(s). Default is 1. You can see more information on Unitor Website."
        Public Shared msg_soundedit_LoopNum_title As String = "Setting Loop Number"

        Public Shared msg_soundedit_notnum_title As String = "Not Numeric"
        Public Shared msg_soundedit_notnum_text As String = "Please input number! Not character(s)."

        Public Shared msg_soundedit_playfail_title As String = "Play Failed"
        Public Shared msg_soundedit_playfail_text As String = "Failed to play the sound. Error Message: "

        Public Shared msg_soundedit_alreadyUsing_title As String = "This sound is using..."
        Public Shared msg_soundedit_alreadyUsing_text As String = "'{0}' is already loaded {1} time(s). Sounds that is already assigned can't be removed. Do you want to remove from assign and delete file?"

        Public Shared msg_soundedit_deletefail_title As String = "Failed to Delete"
        Public Shared msg_soundedit_deletefail_text As String = "Failed to delete '{0}' file. Error Message: "

        Public Shared msg_soundedit_deleteWarning_title As String = "Delete Warning"
        Public Shared msg_soundedit_deleteWarning_text As String = "Do you really want to delete selected sound(s)? This process cannot be undone!"
#End Region

#Region "LEDLoader Error Texts"
        Public Shared str_PackLoad_LED_WrongFormat As String = "There are wrong format"
        Public Shared str_PackLoad_LED_WrongFeatureKey As String = "There are wrong keyLED main command string '{0}' in line #{1} in file name '{2}'."
        Public Shared str_PackLoad_LED_WrongMultiMapChar As String = "There are wrong Multi-Mapping Character in file name '{0}'"
        Public Shared str_PackLoad_LED_WrongkeyLEDFileName As String = "There are wrong keyLED File Name '{0}'"
        Public Shared str_PackLoad_LED_badChainNumber As String = "The chain number of the following file (Chain #{1}) is bigger than the chain number written in info file ({0}). File name is {2}. Unitor ignores this keyLED script file and continues loading."
        Public Shared str_PackLoad_LED_wrongGrammar As String = "Wrong keyLED script ({0}) is in Line#{2} in keyLED file named '{1}'. Unitor ignores this keyLED script line and coninues loading."
#End Region

#Region "LED Editor Strings"
        '이쪽 스트링은 아직 번역되지 않음.
        Public Shared str_lededitor_error_unknowncommand As String = "Unknown Command '{0}'."
        Public Shared str_lededitor_error_wrongaxis As String = "Wrong Axis Code '{0}', '{1}'."
#End Region

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

        Public Shared Sub ReadINI(lang As String)
            If (lang = "Default Translation (English)") Then Exit Sub

            If (My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\translation\" & lang) = True) Then
                Dim Language As String = GetIniValue("TranslationConfig", "Lang", My.Application.Info.DirectoryPath & "\translation\" & lang)
                lang_ = Language
                Dim TrasnlationVer As String = GetIniValue("TranslationConfig", "UnitorVer", My.Application.Info.DirectoryPath & "\translation\" & lang)
                If Not (TrasnlationVer = My.Application.Info.Version.ToString) Then
                    Dim result = MessageBox.Show(lang & " Translation Version (v" & TrasnlationVer & ") is different with Unitor Version (v" & My.Application.Info.Version.ToString & "). Some texts are can't be appeared. Please install other version of translation. Do you want to load texts from Translation file?" & vbNewLine & lang & " 언어의 번역 파일 버전 (v" & TrasnlationVer & ") 이 현재 실행중인 Unitor의 버전 (v" & My.Application.Info.Version.ToString & ") 과 다릅니다. 일부 글자들이 표시되지 않을 수 있습니다. 현재 Unitor의 버전에 맞는 번역 파일을 설치해주세요. 번역 파일에서 텍스트를 로드할까요?", "Different Translation Version / 다른 버전의 번역 파일", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                    If (result = DialogResult.No) Then
                        Exit Sub
                    End If
                End If

                'Load Text From Translation File.
                With MainProjectLoader



                    .TabPad.Text = GetIniValue("TranslationForUnitor", "main_pad", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    '3.0.0.13 추가 번역
                    .TabSoundEdit.Text = GetIniValue("TranslationForUnitor", "main_soundedit", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .TabLEDEdit.Text = GetIniValue("TranslationForUnitor", "main_lededit", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblSoundEdit_intro.Text = GetIniValue("TranslationForUnitor", "main_soundedit_intro", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblSoundEdit_SearchLabel.Text = GetIniValue("TranslationForUnitor", "main_soundedit_soundsearch", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblSoundLib.Text = GetIniValue("TranslationForUnitor", "main_soundedit_soundlib", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblSoundedit_InsideButton.Text = GetIniValue("TranslationForUnitor", "main_soundedit_insidebutton", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnSoundEdit_addlib.Text = GetIniValue("TranslationForUnitor", "main_soundedit_addlib", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnSoundEdit_Reloadlib.Text = GetIniValue("TranslationForUnitor", "main_soundedit_reloadlib", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnSoundEdit_dellib.Text = GetIniValue("TranslationForUnitor", "main_soundedit_dellib", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .TabAbout.Text = GetIniValue("TranslationForUnitor", "main_about", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lbl_information_pakcname.Text = GetIniValue("TranslationForUnitor", "main_informaton_unipackname", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lbl_information_packauthor.Text = GetIniValue("TranslationForUnitor", "main_information_producername", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblInfo_chain.Text = GetIniValue("TranslationForUnitor", "main_information_chains", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .lblInfo_path.Text = GetIniValue("TranslationForUnitor", "main_information_path", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnAction_deletesounds.Text = GetIniValue("TranslationForUnitor", "main_action_deletesounds", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnAction_deleteLED.Text = GetIniValue("TranslationForUnitor", "main_action_deleteleds", My.Application.Info.DirectoryPath & "\translation\" & lang)


                    .checkPad_toggleLED.Text = GetIniValue("TranslationForUnitor", "main_pad_showled", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .toggle_Pad_TestMode.Text = GetIniValue("TranslationForUnitor", "main_pad_testmode", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .toggle_Pad_SoundEdit.Text = GetIniValue("TranslationForUnitor", "main_pad_soundeditmode", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .toggle_Pad_LEDEDit.Text = GetIniValue("TranslationForUnitor", "main_pad_lededitmode", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .lblAbout_UnitorInformation.Text = GetIniValue("TranslationForUnitor", "About_MainText", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnAbout_viewWeb.Text = GetIniValue("TranslationForUnitor", "main_about_viewweb", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .btnAbout_notice.Text = GetIniValue("TranslationForUnitor", "main_about_notice", My.Application.Info.DirectoryPath & "\translation\" & lang)


                    '메뉴 텍스트 변경
                    .FileToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_file", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .OpenToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_file_open", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .SaveToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_file_save", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .SaveWithAnotherNameToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_file_savenewname", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .SettingsToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_file_setting", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .EditToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_edit", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .EditautoPlayToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_edit_autoplay", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .PlayToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_play", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .AutoPlayToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_play_autoplay", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .AutoPlayControlerToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_play_autoplaycontroler", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    .HelpToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_help", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .HelpCenterToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_help_helpcenter", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .OnlineLEDLibraryToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_help_onlineledlib", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .AboutToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_help_about", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    .CheckForAUpdateToolStripMenuItem.Text = GetIniValue("TranslationForUnitor", "main_menu_help_checkforupdates", My.Application.Info.DirectoryPath & "\translation\" & lang)

                    '로딩 폼 초기화
                    Loading.Text = GetIniValue("TranslationForUnitor", "str_Loading_Loading", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    Loading.lblStat.Text = GetIniValue("TranslationForUnitor", "str_Loading_Loading", My.Application.Info.DirectoryPath & "\translation\" & lang)
                    loading_default_windowstring = GetIniValue("TranslationForUnitor", "str_Loading_Loading", My.Application.Info.DirectoryPath & "\translation\" & lang)
                End With

                'MessageBox Texts
                msg_Main_TooManyChains_Text = GetIniValue("TranslationForUnitor", "msgbox_toomanychain_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_TooManyChains_Title = GetIniValue("TranslationForUnitor", "msgbox_toomanychain_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_Load_infoRequirementsNotOK_Text = GetIniValue("TranslationForUnitor", "msgbox_load_notsupported_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_Load_infoRequirementsNotOK_Title = GetIniValue("TranslationForUnitor", "msgbox_load_notsupported_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_0Chain_Text = GetIniValue("TranslationForUnitor", "msgbox_0chain_msg", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_0Chain_Title = GetIniValue("TranslationForUnitor", "msgbox_0chain_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_RemoveChain_Text = GetIniValue("TranslationForUnitor", "msgbox_removechain_warning_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_RemoveChain_Title = GetIniValue("TranslationForUnitor", "msgbox_removechain_warning_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_Load_HigherChain_Text = GetIniValue("TranslationForUnitor", "msgbox_badchainnumber_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_Main_Load_HigherChain_Title = GetIniValue("TranslationForUnitor", "msgbox_badchainnumber_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_dragdropAlreadyExists_Text = GetIniValue("TranslationForUnitor", "msgbox_badchainnumber_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_dragdropAlreadyExists_Title = GetIniValue("TranslationForUnitor", "msgbox_badchainnumber_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_onlywav_Text = GetIniValue("TranslationForUnitor", "msgbox_notExtWav_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_onlywav_Title = GetIniValue("TranslationForUnitor", "msgbox_notExtWav_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_load_noinfo_Text = GetIniValue("TranslationForUnitor", "msgbox_noInfo_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_load_noinfo_Title = GetIniValue("TranslationForUnitor", "msgbox_noInfo_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_load_nokeySound_Text = GetIniValue("TranslationForUnitor", "msgbox_nokeySound_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_load_nokeySound_Title = GetIniValue("TranslationForUnitor", "msgbox_nokeySound_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_main_removekeyled_text = GetIniValue("TranslationForUnitor", "msgbox_removekeyLED_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_main_removekeyled_title = GetIniValue("TranslationForUnitor", "msgbox_removekeyLED_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_main_removesounds_text = GetIniValue("TranslationForUnitor", "msgbox_removekeysound_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_main_removesounds_title = GetIniValue("TranslationForUnitor", "msgbox_removekeysound_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_LoopNum_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_loopnum_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_LoopNum_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_loopnum_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_notnum_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_notnum_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_notnum_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_notnum_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_playfail_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_playfail_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_playfail_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_playfail_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_alreadyUsing_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_alreadySoundRegistered_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_alreadyUsing_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_alreadySoundRegistered_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_deletefail_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_deletefail_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_deletefail_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_deletefail_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_deleteWarning_title = GetIniValue("TranslationForUnitor", "msgbox_soundedit_deleteWarning_title", My.Application.Info.DirectoryPath & "\translation\" & lang)
                msg_soundedit_deleteWarning_text = GetIniValue("TranslationForUnitor", "msgbox_soundedit_deleteWarning_txt", My.Application.Info.DirectoryPath & "\translation\" & lang)

                'Strings
                str_PackLoad_LED_WrongkeyLEDFileName = GetIniValue("TranslationForUnitor", "str_wrongkeyLEDFileName", My.Application.Info.DirectoryPath & "\translation\" & lang)
                str_PackLoad_LED_WrongFeatureKey = GetIniValue("TranslationForUnitor", "str_wrongkeyLEDFeature", My.Application.Info.DirectoryPath & "\translation\" & lang)
                str_PackLoad_LED_WrongFormat = GetIniValue("TranslationForUnitor", "str_wrongkeyLEDFormat", My.Application.Info.DirectoryPath & "\translation\" & lang)
                str_PackLoad_LED_WrongMultiMapChar = GetIniValue("TranslationForUnitor", "str_wrongkeyLEDMultiMapChar", My.Application.Info.DirectoryPath & "\translation\" & lang)
                str_PackLoad_LED_badChainNumber = GetIniValue("TranslationForUnitor", "str_badChainNumber", My.Application.Info.DirectoryPath & "\translation\" & lang)
                str_PackLoad_LED_wrongGrammar = GetIniValue("TranslationForUnitor", "str_wrongLEDLine", My.Application.Info.DirectoryPath & "\translation\" & lang)

                'Loading
                loading_initdir = GetIniValue("TranslationForUnitor", "str_loading_initdir", My.Application.Info.DirectoryPath & "\translation\" & lang)
                loading_keySoundLoading = GetIniValue("TranslationForUnitor", "str_loading_keysoundLoad", My.Application.Info.DirectoryPath & "\translation\" & lang)
                loading_unzipping = GetIniValue("TranslationForUnitor", "str_loading_unzipping", My.Application.Info.DirectoryPath & "\translation\" & lang)
                loading_keyLEDLoading = GetIniValue("TranslationForUnitor", "str_loading_keyledload", My.Application.Info.DirectoryPath & "\translation\" & lang)
                loading_removeChain = GetIniValue("TranslationForUnitor", "str_loading_removechain", My.Application.Info.DirectoryPath & "\translation\" & lang)
                loading_removeWorkspace = GetIniValue("TranslationForUnitor", "str_loading_removeWorkspace", My.Application.Info.DirectoryPath & "\translation\" & lang)

                loading_load_converting = GetIniValue("TranslationForUnitor", "str_loading_converting", My.Application.Info.DirectoryPath & "\translation\" & lang)
            Else
                MessageBox.Show(lang & " Translation doesn't exists in Unitor Translation directory. Please install it. All texts will be shown in English." & vbNewLine & lang & " 언어의 번역 파일이 Unitor의 번역 파일 폴더에 없습니다. 언어 번역 파일을 설치해주세요. 모든 텍스트가 영어로 표시됩니다.", "No translation file / 번역 파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Information)

            End If

        End Sub

        Private Shared Function GetTextEncodingInfo(ByVal path As String) As Encoding
            Dim enc As Encoding

            Using sr As StreamReader = New StreamReader(path, True)
                enc = sr.CurrentEncoding
                sr.Close()
            End Using

            Return enc
        End Function
    End Class
End Namespace