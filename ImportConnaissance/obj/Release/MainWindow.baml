   M S B A M L   `   `   `  ���� @  <Wanao, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WanaoWanao  E AWanao_Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;,clr-namespace:Wanao_Core;assembly=Wanao_Core
Wanao_Core H DMahApps.Metro, Version=1.4.0.0, Culture=neutral, PublicKeyToken=nullV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls     Wanao.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     %localclr-namespace:Wanao     C
Wanao_Core,clr-namespace:Wanao_Core;assembly=Wanao_Core     PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     -   $��1281.4��$��1868.2��$	��600��$	��600��  � Title$  PDF2AllotStd�� � Icon$ Images/sigle_bulle(4).ico�� � DataContext �� *Import+   Locator �� Source# [  .��� ���� -   BackgroundImageBrush�� �� ImageSource$ Images/Background.png�� � RowDefinitions �� $��64 ��� $��* � � ColumnDefinitions �� $	��140 ��� $��* �.+��� $��0��$��0��$��2��$��8q����	#60FFFFFF._�� .+��� $��4q� �� Orientation$ 
Horizontal=�.+��� $��Images/pooldownload.png��$��4,4,8,4q��� .��PoolDownLoad Massive�� $��0��$��1��$��8,0,8,8q����	#60FFFFFF._��� $
��Auto=�.���� $ Vertical=�$��0,8,0,8q�.+�!  MahApps.Metro.Controls.Tile  -     Name$ 	dashboard��
����������	#A0800080$��0,4,0,4q�$	��100��$	��100��	   
TiltFactor$	 2��
   Title$
  ��$
��HOME��.���� $ Vertical=�$��8q�.+��� $#��Images/Loading - 01.Black.png��$��60���� $��	Dashboard��
�������$��Center=��� -    ��Name$ cmbchoix��$	��120��.@��� .��Files�� .��Test to compare�� .��Test to compare (reduit 20)�� .��
Test url  -   $	 run��
����������	#A0800080$��0,4,0,4q�$	��100��$	��100��$	 2��$
  ��$
��HOME��.���� $ Vertical=�$��8q�.+��� $#��Images/Loading - 01.Black.png��$��60���� $	��Run��
�������$��Center=�  -   $ bdreset��
����������	#A0800080$��0,4,0,4q�$	��100��$	��100��$	 2��$
  ��$
��HOME��.���� $ Vertical=�$��8q�.+��� -    ��Name$ 
imgbdreset��$#��Images/Loading - 01.Black.png��$��60���� $��Reset BD��
�������$��Center=�  -   $ bdview��
����������	#A0800080$��0,4,0,4q�$	��100��$	��100��$	 2��$
  ��$
��HOME��.���� $ Vertical=�$��8q�.+��� -	   $ 	imgbdview��$#��Images/Loading - 01.Black.png��$��60���� $��View BD��
�������$��Center=��� -
    ��Name$ brdgrid��$��1��$��1��
���   �$��2q�$��10,10,81.6,72q�._��� -    ��Name$ 
swfileinfo��$��1��$��1��$
��Auto=�$
��Auto=���. $��8.4,8.4,52.2,67.8q�.��� -    �Name$ fileinfo��$��1��$��1��$	��Top=�$
��Left=�$
��Auto=�$
��Auto=���. $
��Auto��$
��1435���� -   $ brddatagrid��$��1��$��1��
���   �$��2q�$��10,10,81.6,72q�$y�Hidden=�._��� -   $ 
swdatagrid��$
��Auto=�$
��Auto=�$��8.4,8.4,52.2,67.8q�.��&   System.Windows.Controls.DataGrid  -     Name$ dataGrid��   AutoGenerateColumns . $
��Left=�$��	10,10,0,0q�$
��Auto��$
��1435���� -    ��Name$ 	txtstatus��$��1��$
��Left=�$��57��$��10,1121,0,0q�$��1��$	��Top=�$
��1630���� -   $ 
txtConsole��$��1��$
��Left=�$��20��$��	30,31,0,0q�$��1�� �� TextWrapping$
 Wrap=�$	��Top=�$	��100��$y�Hidden=����� *NomURl+� $��1��$
��Left=�$	��100��$��650,1178,0,-90q�$��1��$	��Top=�$	��100��