   M S B A M L   `   `   `  ����   TWD Voice Demo&     TwinsDay2017_VoiceDemo.UserInput   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Input Prompt�� � SizeToContent$ WidthAndHeight=�$	��500��$	��350�� � 
ResizeMode$ NoResize=� � ShowInTaskbar .  � WindowStartupLocation$ CenterOwner=�.���  � RowDefinitions �� $��* ��� $
��Auto ��� $
��Auto �.+�� $��0��$��10q� � ColumnDefinitions �� $��* ��� $��3* �.+��� -   $��	InputIcon��$��0��$
��Left=�$��40��$��Center=��� -   $��InputQuestion��$��1��$��15(�$��SemiBold"�$��Italic$�$
��10,0q�$��WrapWithOverflow=�$��Center=�� $��1��$��10q� �� $
��Auto ��� $��* �.+��� -   $��
TextPrompt��$��0��$
��Left=�$��Input: ��$��13(�$��Medium"��� -    ��Name$ InputWrapper��$��1��$��5,0,10,0q��� -   $��UserTextInput��$��1��$��30��$��5,0,10,0q�$��Center=� �� TextWrapping$ NoWrap=����� *ActualWidth+	 �� ElementName$	 InputWrapper���� $��2��
 �� Orientation$
 
Horizontal=�$��Center=�$��
20,5,20,10q�.+��� -   $��InputOK��$��Ok�� �� 	IsDefault . $��50��$��30��$	��5,0q��� -   $��InputCancel��$��Cancel�� �� IsCancel . $��80��$	��5,0q�