   M S B A M L   `   `   `  ����N  JPoolDownloadMassive, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WanaoWanao  5      E AWanao_Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;,clr-namespace:Wanao_Core;assembly=Wanao_Core
Wanao_Core 5      H DMahApps.Metro, Version=1.4.0.0, Culture=neutral, PublicKeyToken=nullV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls 5	          Wanao.MainWindow   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      %localclr-namespace:Wanao     5      C
Wanao_Core,clr-namespace:Wanao_Core;assembly=Wanao_Core     5      PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5	      -   5        $��1281.4��5      $��1868.2��6   $	��600��6#   $	��600��62     � Title$  PoolDownLoadMassive��5       � Icon$ Images/pooldownload.ico��6     � DataContext 5      �� *Import+   Locator �� Source# [  .��5      � ��5   
   �� 5      -   5        BackgroundImageBrush�� �� ImageSource$ Images/Background.png��5   7    � RowDefinitions 5   
   �� 5      $��64 �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $	��140 �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��0��6!   $��2��6.   $��8q�6B   ���	#60FFFFFF._�5      � $��10,0,73.6,0q�6   .+�5       �� $��4q�6    �� Orientation$ 
Horizontal=�6(   .+�5!      �� $��4,4,8,4q�6   $��Images/pooldownload.ico��6.    � ToolTip$ PoolDownLoad Massive��6O   �� 5"      $��15,4,8,4q�6   $��PoolDownLoad Massive��6.   $	��250��6M   5#      5$      5%      �� 5'   
   $��0��6   $��1��6!   $��8,0,8,8q�6.   ���	#60FFFFFF._�5(      �� $
��Auto=�6   .��5)      �� $ Vertical=�6   $��0,8,0,8q�64   .+�5*      !  MahApps.Metro.Controls.Tile  -   5        	  Name$	 	dashboard��5*   $   
����������	#A0800080$��0,4,0,4q�5,      $	��100��6(   $	��100��64   
   
TiltFactor$
 2��6A      Title$  ��5-      $
��HOME��5.      .��5/      �� $ Vertical=�6%   $��8q�6<   .+�50      �� $#��Images/Loading - 01.Black.png��6$   $��60��6K   �� 51      $��	Dashboard��6(   
�������$��Center=�6L   52      53      �� 54      -   5         ��Name$ txtNbRec��54      $��0��60   $��1��6A   $��5��6N   $
��Left=�6X   $	��Top=�6s   $��20��6�   $��40��6�   �� 55      -   5         ��Name$ cmbchoix��55      $	��120��61   .@�56      �� .��6'   Files6.   �� 57      .��6'   Test to compare68   �� 58      .��6'   Test to compare (reduit)6A   �� 59      .��6'   
Test url61   5:        5;      -   5        $		 run��5;   $   
����������	#A0800080$��0,4,0,4q�5=      $	��100��6*   $	��100��66   $
 2��6C   $  ��5>      $
��HOME��5?      .��5@      �� $ Vertical=�6%   $��8q�6<   .+�5A      �� $#��Images/Loading - 01.Black.png��6$   $��60��6K   �� 5B      $	��Run��6(   
�������$��Center=�6F   5C      5D        5E      -   5        $	 bdview��5E   $   
����������	#A0800080$��0,4,0,4q�5G      $	��100��6*   $	��100��66   $
 2��6C   $  ��5H      $
��HOME��5I      .��5J      �� $ Vertical=�6%   $��8q�6<   .+�5K      �� -   5         ��Name$ 	imgbdview��5K   $   $#��Images/Loading - 01.Black.png��67   $��60��6^   �� 5L      $��View BD��6(   
�������$��Center=�6J   5M      5N      5O      5Q      5R      �� 5U   
   -	   5         ��Name$ brdgrid��5U   0   $��1��6   $��1��6"   
���   �$��2q�6U   $��10,10,81.6,72q�6i   ._�5V      �� -
   5         ��Name$ 
swfileinfo��5V      $��1��6/   $��1��6@   $
��Auto=�5W      $
��Auto=�6>   ��. $��8.4,8.4,52.2,67.8q�5X      .��5Y      � -   5         �Name$ fileinfo��5Y      $��1��6*   $��1��6:   $	��Top=�5Z      $
��Left=�60   $
��Auto=�5[      $
��Auto=�6J   ��. $
��Auto��5\      $
��1435��6&   5]      5^      �� 5a   
   -   5        $ brddatagrid��5a   /   $��1��6   $��1��6"   
���   �$��2q�6Y   $��10,10,81.6,72q�6n   $y�Hidden=�6�   ._�5b      �� -   5        $ 
swdatagrid��5b      $
��Auto=�5c      $
��Auto=�6>   $��8.4,8.4,52.2,67.8q�5d      .��5e      &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��5e         AutoGenerateColumns . $
��Left=�5f   6   $��	10,10,0,0q�5g      $
��Auto��6.   $
��1435��6<   5h      5i      �� 5l   
   -   5        $ 	txtstatus��5l      $��1��6%   $
��Left=�65   $��57��6P   $��10,1121,0,0q�6\   $��1��6q   $	��Top=�6~   $
��1630��6�   �� 5o   
   -   5        $ 
txtConsole��5o      $��1��6&   $
��Left=�66   $��20��6Q   $��	30,31,0,0q�6]   $��1��6p    �� TextWrapping$
 Wrap=�6}   $	��Top=�6�   $	��100��6�   $y�Hidden=�6�   ��6�   �� *NomURl+6�   � 5p   
   $��1��6   $
��Left=�6$   $	��100��6?   $��650,1178,0,-90q�6L   $��1��6d   $	��Top=�6q   $	��100��6�   5r      5u      