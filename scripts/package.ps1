# Lyra_version.zip �쐬�X�N���v�g
# TODO: �g���w��݂̂ŃR�s�[�ł���悤��(*.dll, *.exe, *.config)

# �f�B���N�g���쐬
New-Item artifacts\Lyra -ItemType Directory

# �R�s�[
Copy-Item Lyra\bin\Debug\* artifacts\Lyra -Recurse

# �S�~�����������̂́A���[�g�݂̂Ȃ̂ł��������ɏ���
Get-ChildItem artifacts\Lyra\ -Include *.pdb,*.xml -Recurse | Remove-Item