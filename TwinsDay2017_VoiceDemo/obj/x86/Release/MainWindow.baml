   M S B A M L   `   `   `  ���� [  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms  I ETWD Voice Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:TwinsDay2017_VoiceDemoTwinsDay2017_VoiceDemo '   !TwinsDay2017_VoiceDemo.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     4src$clr-namespace:TwinsDay2017_VoiceDemo     -     � Title$9  3WVU Biometrics Voice Verfication and Identification��$	��800��$
��1200�����#EAAA00.��� -   $��MainGrid��$��20,10,10,10q����#193D66|�.  � FocusVisualStyle Z�  � ColumnDefinitions �� �� $
��Auto � � RowDefinitions �� �� $
��Auto �.+��� -   $��5q�
�������$��2��.@��� -    ��Name$ IdentifyTab��$��	0,0,0,2.5q�$��50������ $��Identification��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� �� $��2* ��� $��3* �.+�]� ��. $��0��.+��� $	��Top=�$��2,2,2,3q����#6E6259._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -   $��IdentifyImport��$	��100��$��28��$��3q�$	��Top=�$��Right=�.���� $��24������  �� DecodePixelHeight$ 24�� �� 	UriSource$( "/Icon Images/Normal/FolderPlus.png?��� -   $��IdentificationImportPath��$	��Top=�$��Center=�$��28��$��3q�]� $��0����. .+��� -   $��IdentifyScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -   $��IdentificationScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+���  �� Orientation$ 
Horizontal=�$��Right=�.+��� -	   $��IdentifySelect��$��Select��$��60��$��25��$��3q�{�.  �� -
   $��IdentifyDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -   $��IdentifyVoiceSelect�� �� Text$ Select Image...��$��3q�{�.  ]� $��Bottom=����#325276.+��� $ 
Horizontal=�$��Center=�$��Bottom=�$��40��$��0,0,0,5q�.+��� -   $��IdentificationClearWindow��$��Clear��$��62��$��0,0,5,0q�$��16(�{�.  �� -   $��IdentificationReset��$��Reset��$��62��$��5,0,0,0q�$��16(�{�.  �� -   $��GenerateMatches��$��Generate Matches��$��52��$��3q�$��18(�{�.  �� $��2,0,2,2q����#6E6259._�]� ��. .+�� ���#807F7E �� $��* ��� $��* �.+�]� $��0��$��Center=����#565453.+��� -   $��IdentificationRID��$	��---��$��16(�$	��Top=�$��Center=�
��������� -   $ ��IdentificationPlayerStatus��$��No File Loaded��$��16(�$��Italic$�$	��Top=�$��Center=�
�������� $��Center=�$��Bottom=�$��0,0,0,10q�.+��� -   $��	IdentPlay��$��52��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Play.png?��� -   $��
IdentPause��$
��10,0q�$��55��$��35��.���� $��24������ $ 24��$# /Icon Images/Normal/Pause.png?��� -   $��	IdentStop��$��51��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Stop.png?��� -   $��	IdentTime��$
��10,5q�	 �� IsMoveToPointEnabled	 .  ]� $��1��$��Center=����#565453.+��� -   $��	ResultRID��$	��---��$��16(�$	��Top=�$��Center=�
��������� -   $��ResultPlayerStatus��$��No Result Loaded��$��16(�$��Italic$�$	��Top=�$��Center=�
�������� $��Center=�$��Bottom=�$��0,0,0,10q�.+��� -   $��
ResultPlay��$��52��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Play.png?��� -   $��ResultPause��$
��10,0q�$��55��$��35��.���� $��24������ $ 24��$# /Icon Images/Normal/Pause.png?��� -   $��
ResultStop��$��51��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Stop.png?��� -   $��
ResultTime��$
��10,5q�]� ��. $��1��$��20,0,0,0q�.+��� $	��Top=�$	��500��$��0,10,0,0q�$��2q����#6E6259���#D0D4D6._��� $��Top Matches:��$��16(�$��Center=�$	��Top=�$��0,3,3,0q�$��Black"�$��Italic$�� $��Bottom=�$��3q�.+�]� .+��� -   $��IdentStatusInfo��$��Bottom=�$��
Progress: ��$��25��
�������$��Center=�
 �� IsReadOnlyCaretVisible
 .   �� 
IsReadOnly . $��Black"�$��Italic$�� -   $��IdentificationStatus�� � Minimum$ 0Q� � Maximum$	 100Q� � Value$ 0Q�$��35���� -   $��TopMatchList��   resultListStyle#��[  ���#6E6259$��2q�$��3q�
���������. $��Visible=����� ���� .R�]�  ]�Name$ 	MatchItem��-   $��3q�.+�� $
��Left=� �� $��50���� $��60 ��� $
��Auto �.+��� $��0��$	��5,0q�$
��Bold"�$��20(�$��Center=�$��Center=�����  �� Path$
 Rank� �� StringFormat$
 #{0}���� -   $��1��$��40��$��50��
 � Tag �� $
 Rank�.���� Load�� Audio�� -    $��2�� � Cursor$
 Hand�� �� $
 Rank�.���� $	��165������ $ Image�� $
��10,0q� �� $��* ��� $��* ��� $��* ��� $��* �.+��� $��0��$ 
Horizontal=�$��Center=�.+��� $��0q�$��16(�$
��Bold"�$��Italic$�$��RID: ���� $��0q�$��16(����� $ ID��� $��1��$ 
Horizontal=�$��Center=�.+��� $��0q�$��16(�$
��Bold"�$��Italic$�$��Score: ���� $��0q�$��16(����� $ Score��� $��2��$ 
Horizontal=�$��Center=�.+��� $��0q�$��16(�$
��Bold"�$��Italic$�$��False Acceptance Rate: ���� $��0q�$��16(����� $	 FAR�$ {0:0.####}%���� $��3��$ 
Horizontal=�$��Center=�.+��� $��0q�$��16(�$
��Bold"�$��Italic$�$$��False Acceptance Probability: ���� $��0q�$��16(����� $ Probability�$ {0:0.####}%������  �� 
TargetType" �� �� Triggers P�  P� Property �@��
IsSelected P� Value . . ���  �� Property � �� �� Value
 �   ��� -!   $ 	VerifyTab��$��0,2.5q�$��50������ $��
Verify 1:1��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� �� $��2* ��� $��* �$	��250��.+�� $��0�� �� �� .+�]� $��0����. $	��0,3q�.+��� $	��Top=�$��2q����#6E6259._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -"   $��VerificationImportTop��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -#   $��VerificationImportPathTop��$��3q�$��Center=�]� $��0����. .+��� -$   $��TopScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -%   $��TopScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -&   $��	TopSelect��$��Select��$��60��$��25��$��3q�{�.  �� -'   $��TopDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -(   $��TopVoiceSelect��$ Select Voice...��$��3q�{�.  �� $��2,0,2,2q����#6E6259._�� ���#807F7E �� $��* �.+�]� $��0�����#565453$��Center=�.+��� -)   $��TopRID��$	��---��$��16(�$	��Top=�$��Center=�
��������� -*   $��TopPlayerStatus��$��No File Loaded��$��16(�$��Italic$�$	��Top=�$��Center=�
�������� $��Center=�$��Bottom=�$��0,0,0,10q�.+��� -+   $��TopPlay��$��52��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Play.png?��� -,   $��TopPause��$
��10,0q�$��55��$��35��.���� $��24������ $ 24��$# /Icon Images/Normal/Pause.png?��� --   $��TopStop��$��51��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Stop.png?��� -.   $��TopTime��$
��10,5q�]� $��1����. $	��0,3q�.+��� $	��Top=�$��2q����#6E6259._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -/   $��VerificationImportBottom��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -0   $"��VerificationImportPathBottom��$��3q�$��Center=�]� $��0����. .+��� -1   $��
BottomScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -2   $��BottomScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -3   $��BottomSelect��$��Select��$��60��$��25��$��3q�{�.  �� -4   $��BottomDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -5   $��BottomVoiceSelect��$ Select Voice...��$��3q�{�.  �� $��2,0,2,2q����#6E6259._�� ���#807F7E �� $��* �.+�]� $��0�����#565453$��Center=�.+��� -6   $��	BottomRID��$	��---��$��16(�$	��Top=�$��Center=�
��������� -7   $��BottomPlayerStatus��$��No File Loaded��$��16(�$��Italic$�$	��Top=�$��Center=�
�������� $��Center=�$��Bottom=�$��0,0,0,10q�.+��� -8   $��
BottomPlay��$��52��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Play.png?��� -9   $��BottomPause��$
��10,0q�$��55��$��35��.���� $��24������ $ 24��$# /Icon Images/Normal/Pause.png?��� -:   $��
BottomStop��$��51��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Stop.png?��� -;   $��
BottomTime��$
��10,5q�]� $��1��$��3q���. .+�]� $	��Top=����#325276.+�� $	��Top=�$	��5,5q����#A2AAAD �� �� �� �� �� .+��� $��0��$��Extraction Options: ��$��14(�$��Italic$�$��SemiBold"��� $��0��$ ��Icon Images/Small/Info.png��$��16��$
 Help��$��Right=�$
��10,0q� ^� InitialShowDelay$
 1000�� ^� ShowDuration$ 10000�� ^� BetweenShowDelay$
 2000�� ^� 	IsEnabled .  � ToolTip _� $	��350��.���� $
��Wrap=���Extraction options must be selected before importing or scanning in any subjects. If you have created a subject, you must clear all (both top and bottom subject) before being able to alter extraction options.]� $��1����. $
��15,0q�.+��� -<   $��TextDependentCheck��$
��Left=�$��Center=� d� 	IsChecked$
 TrueY��� $��Text-Dependent��]� $��2����. $
��15,0q�.+��� -=   $��TextIndependentCheck��$
��Left=�$��Center=��� $��Text-Independent���� $��3��$��Verification Options:��$��14(�$��Italic$�$��SemiBold"�]� $��4����. $
��15,0q�.+��� ->   $��UniqueCheck��$
��Left=�$��Center=��� $��Unique Phrases Only���� -?   $��VerifyVoices��$��Verify��$��3q�$	��Top=�{�.  $��40��$��18(��� $��Bottom=�$��	0,2.5,0,0q����#325276.+��� -@   $��VerificationStore��$y�	Collapsed=�$��Store Results��{�.  $��55��$	��2.5q�$��18(��� $ 
Horizontal=�$��Center=�$��45��.+��� -A   $��VerificationClearTop��$	��2.5q�$��50��{�.  .����   �� TextAlignment$  Center=�Clear�� Top�� -B   $��VerificationClearBottom��$	��2.5q�$��50��{�.  .���� $  Center=�Clear�� Bottom�� -C   $��VerificationClear��{�.  $	��2.5q�$��50��.���� $  Center=�Clear�� Both�� -D   $��VerificationReset��$��Reset��{�.  $	��2.5q�$��50���� ���#6E6259$��2q�$	��0,3q�._�]� ��. .+��� $��Results:��$	��Top=����#D0D4D6$��16(�$��Black"�$��Italic$��� -E   $��VerifyResults��$��16(�
�������! �� TextWrapping$
! Wrap=�
 .  . ��. $��Visible=��� -F   $ 
VerifyNTab��$��0,2.5q�$��50������ $��
Verify 1:N��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� �� $��* ��� $��* �.+�]� $��0��.+�]� $��Bottom=����#325276.+��� $ 
Horizontal=�$��40��$��Center=�$��Bottom=�$��0,0,0,5q�.+��� -G   $��VerifySubject_Clear��$��Clear��$��80��$��0,0,5,0q�$��16(�{�.  �� -H   $��VerifySubject_Reset��$��Reset��$��80��$��5,0,0,0q�$��16(�{�.  �� -I   $��VerifySubjectVoice��$	��Top=�$��Verify��$��53��$��3q�$��18(�{�.  ]� $	��Top=�.+��� $	��Top=�$��2q����#6E6259._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -J   $��VerifyImport��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -K   $��VerifyImportPath��$��3q�$��Center=�]� $��0����. .+��� -L   $��
VerifyScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -M   $��VerifyScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -N   $��VerifySelect��$��Select��$��60��$��25��$��3q�{�.  �� -O   $��VerifyDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -P   $��VerifyVoiceSelect��$ Select Voice...��$��3q�{�.  �� $��2,0,2,2q����#6E6259._�� ���#807F7E �� $��* �.+�]� $��0�����#565453$��Center=�.+��� -Q   $��	VerifyRID��$	��---��$��16(�$	��Top=�$��Center=�
��������� -R   $��VerifyPlayerStatus��$��No File Loaded��$��16(�$��Italic$�$	��Top=�$��Center=�
�������� $��Center=�$��Bottom=�$��0,0,0,10q�.+��� -S   $��
VerifyPlay��$��52��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Play.png?��� -T   $��VerifyPause��$
��10,0q�$��55��$��35��.���� $��24������ $ 24��$# /Icon Images/Normal/Pause.png?��� -U   $��
VerifyStop��$��51��$��35��.���� $��24������ $ 24��$" /Icon Images/Normal/Stop.png?��� -V   $��
VerifyTime��$
��10,5q�]� $��1��$��15,0,0,0q�.+��� -W   $��VerifyStoreResult��$y�	Collapsed=�$��Bottom=�$��Store Results��$��55��$��10q�$��16(�{�.  �� $	��Top=�$��3,10,3,0q����#D0D4D6$��2q����#6E6259._��� $��Results:��$	��0,3q�$��Center=�$��16(�$
��Bold"�$��Italic$��� -X   $��VerifyN_Results��$��16(�
�������$��3q����#6E6259$��2q���. $��Visible=�$
! Wrap=�
 .  . �� -Y   $ TemplateTab��$��0,2.5q�$��50������ $��Generate Templates��.���� $��-5q�$��3q����#2C2A29���#002855._�]� $��4q�.+�]� $��Bottom=�.+�� $��Right=����#325276 �� $��50 ��� $��35 �.+��� $��0��$ 
Horizontal=�$��Right=�.+��� -Z   $��GenerateAll��$��Generate All Templates��$	��138��$��40��$��5,5,2.5,2.5q��� -[   $��GenerateSelected��$!��Generate Selected Templates��$	��173��$��40��$��2.5,5,5,2.5q��� -\   $��CancelTemplate��$��1��$��Cancel��{�.  $��	5,2.5,5,5q�� $
��Left=�$
��20,5q�.+�]� .+��� -]   $��TemplateStatusInfo��$��Bottom=�$��
Progress: ��$��25��$��Center=�
 .   . $��Black"�$��Italic$�
�������� -^   $��TemplateStatus��$ 0Q�$	 100Q�$ 0Q�$��40��]� $	��Top=����#A2AAAD.+��� $��Bottom=�$��4q�$
��Bold"�$��Italic$�$(��"Select directories to be searched:���� 
�������$��1q����#B0B0B0$��
10,10,10,0q�" J� Effect" 3  -System.Windows.Media.Effects.DropShadowEffect  #   
BlurRadius$# 2Q�$   ShadowDepth$$ 2Q�%   Color$% DarkGray��._��� $��5q�.+��� $��14(�$��SemiBold"�$��Italic$�$��5q�Please Note:�� $��14(�$
��Wrap=�$��5q���Template generation will traverse through selected directories to record subject information and generate a biometric template for that given subject. When evaluating selected directories, only subjects and data within those directories will be re-evaluated. Template data is used for identification, so any subjects that have not yet been evaluated will be omitted during the enrollment phase of the identification process.�� -_   $��DatabaseDisplay��& �� SelectionMode$& Multiple=����#6E6259$��2q�
�������$	��0,4q���. $��Visible=��� -`   $ SettingsTab��$��0,2.5q�$��50������ $��Settings��.���� $��-5q�$��3q����#2C2A29���#002855._�]� $��4q�.+��� $ 
Horizontal=�$��Right=�$��Bottom=�$��Bottom=�$��36�����#325276.+��� -a   $��ApplySetting��$��Apply��$��90��$��2.5,0q�{�.  �� -b   $��CancelSetting��$��Cancel��$��90��$��2.5,0q�{�.  �� -c   $��RevertSetting��$��Revert��$��90��$��2.5,0q�{�.  �� -d   $��DefaultSetting��$��Default��$��90��$��2.5,0q�{�.  �� $	��Top=�"   $# 2Q�$$ 5Q�._�]� ���#A2AAAD.+��� $	��Top=�$��8q�
�������$��1q����#B0B0B0"   $# 2Q�$$ 2Q�$% DarkGray��._��� .+��� $��14(�$��SemiBold"�$��Italic$�$��5q�Please Note:�� $��14(�$
��Wrap=�$��5q���When changing any settings or adding/removing directories, make sure to apply them or your changes will not be retained. If, for some reason, you accidentally apply an unwanted change, you can select the Revert button to return back to the previously applied settings.]� $��Bottom=�$��	0,7.5,0,5q�.+��� $ 
Horizontal=�$��Right=�.+��� $��Set TWD 2017 Server:��$��Center=�$��Black"�$��Italic$�$
 Help��$
 1000��$ 10000��$
 2000�� .  _� $	��350��.���� .+��� $
��Bold"�Twins Day 2017 Server�� $
��Wrap=���Creates a dialog box to set the 2017 Twins Day server. When the dialog box opens, it will already display the currently set server addesss. Please note that the address entered should correspond to the root address of the server, not the address to a modality folder.�� �� ��The server set will correspond to image/voice aquisition for both template processing and for demo purposes, such as obtaining probe faces/voices for a specific RID.�� -e   $��
SetTWD2017��$��45��$
��10,0q�.���� $��24������ $ 24��$" /Icon Images/Normal/Gear.png?��� $ 
Horizontal=�$
��Left=�.+��� $#��Directories to Match Against:��$��Center=�$��Black"�$��Italic$�$
 Help��$
 1000��$ 10000��$
 2000�� .  _� $	��350��.���� .+��� $
��Bold"�Directories/Networks�� $
��Wrap=���The list of directories used to search for and record subjects when generating templates. If there is a directory or network path that is not present, you can search through folder structures (Add Directory) or add a network by addesss (Add Network/Server).�� �� ��Please note that directory and network paths should point to the folders which contain all of the RID folders. If this is not the case, it may cause errors during the template process.�� -f   $��AddDirectory��$��Add Directory��$��32��$	��105��$��2.5,0q��� -g   $��AddNetworkDrive��$��Add Network/Server��$	��148��$��2.5,0q��� -h   $��RemoveDatabase��$��Remove Selected��$��32��$	��130��$��2.5,0q��� -i   $��ClearDatabase��$��	Clear All��$��32��$��70��$��2.5,0q��� $ 
Horizontal=�$��	0,5,0,7.5q�.+��� $'��!Number of Top Matches to Display:��$��2q�$��Black"�$��Italic$�$
 Help��$
 1000��$ 10000��$
 2000�� .  _� $	��350��.���� .+��� $
��Bold"�Top Matches�� $
��Wrap=���The top matches will set the maximum possible results returned during the identification process. However, if there are fewer matches than the maximum, only those top matches will be displayed.�� -j   $��NumOfMatches��' �� 
IsEditable' . ( �� IsTextSearchEnabled( .  $��60��$��2q��� $,��&Matching False Acceptance Rate ( % ): ��$��5,2,2,2q�$��Black"�$��Italic$�$
 Help��$
 1000��$ 10000��$
 2000�� .  _� $	��350��.���� .+��� $
��Bold"�False Acceptance Rate (FAR)�� $
��Wrap=���FAR is the percent likelihood that an incorrect match will be accepted as a match. So if FAR is set larger, then it will be more likely that a false match will be made.�� �� ��The FAR is related to the matching threshold. So the higher the FAR is, the lower the matching score/threshold will be, and vice versa. To finalize any changes, either press enter or click outside the control.�� -k   $��setMatchFAR��' . ( .  $	��100��$��2q��� $$��Matching Threshold ( Score ): ��$��5,2,2,2q�$��Black"�$��Italic$�$
 Help��$
 1000��$ 10000��$
 2000�� .  _� $	��350��.���� .+��� $
��Bold"�	Threshold�� $
��Wrap=���The matching score/threshold is the minimum score that both identification and verification functions use to assume that a subject is a match. Any comparisons that return a comparison score that is below the threshold, the resulting score will be set to zero.�� �� ��The threshold is related to the FAR. So the higher the score, the lower the FAR, and vice versa. To finalize any changes, either press enter or click outside the control.�� -l   $��setThreshold��$��75��$��2q�� $	��0,4q� �� $��40 ��� $��* �.+��� -m   $��DBStatusList��$��0����. $��Disabled=�$��Disabled=�$��2,2,1,2q����#6E6259
����������� $ Items�) �� ElementName$) DatabaseDisplayList������ .R��� -n   $��25��$��25��$	��2.5q�
����·�$��Center=�$��Center=� �� * �� Mode$* OneWay=�.���� $��16������ + �� DecodePixelWidth$+ 16��$$ /Icon Images/Small/Refresh.png?����� " ��.���  � |� .  ��  � ��$ Center=��� -o   $��DatabaseDisplayList��$��1��$& Multiple=�$��0,2,2,2q����#6E6259
���������. $��Visible=����� " ��.���  � ��$ 30���� -p   $ LogTab��$��0,2.5q�$��50������ $
��Logs��.���� $��-5q�$��3q����#2C2A29���#002855._��� -q   $��LogText��$��10q���. $��Visible=�$
! Wrap=�
 .  . 
����������#6E6259$��2q�