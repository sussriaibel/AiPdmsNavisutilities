@echo off

rem --------------------------------------------------------------------------------
rem  set evar for current application name
rem --------------------------------------------------------------------------------
set applic=PdmsAdmin


set InitBatch=%~dp0..\..\..\..\Code\Batch\InitBatch.bat
call  %InitBatch%

rem --------------------------------------------------------------------------------
rem firstmacfile section
rem --------------------------------------------------------------------------------

set macFile=firstFile.mac
set LblFil=%~dp0..\log\%macFile%.lbl
echo %DATE% %TIME% > %LblFil%
call %msw_dir%\Batch\launch.bat tty %proj% %gene_rwuser% %gene_mdb% $m/%macdir%/%macFile% 

:firstmacfile
timeout /t 3 /nobreak > NUL
if exist %LblFil% goto :firstmacfile

rem --------------------------------------------------------------------------------
rem secondmacfile section
rem --------------------------------------------------------------------------------
set macFile=secondFile.mac
set LblFil=%~dp0..\log\%macFile%.lbl
echo %DATE% %TIME% > %LblFil%
call %msw_dir%\Batch\launch.bat tty %proj% %gene_rwuser% %gene_mdb% $m/%macdir%/%macFile% 

:secondmacfile
timeout /t 3 /nobreak > NUL
if exist %LblFil% goto :secondmacfile

rem --------------------------------------------------------------------------------
rem thirdmacfile section
rem --------------------------------------------------------------------------------
set macFile=thirdFile.mac
set LblFil=%~dp0..\log\%macFile%.lbl
echo %DATE% %TIME% > %LblFil%
call %msw_dir%\Batch\launch.bat tty %proj% %gene_rwuser% %gene_mdb% $m/%macdir%/%macFile% 

:thirdmacfile
timeout /t 3 /nobreak > NUL
if exist %LblFil% goto :thirdmacfile

rem End of script