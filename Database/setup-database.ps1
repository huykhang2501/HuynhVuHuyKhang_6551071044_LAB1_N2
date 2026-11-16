param(
    [string]$Server = '.\SQLEXPRESS',
    [string]$SourcePath = 'D:\Download\Database.sql',
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $SourcePath)) { throw "SQL source file was not found: $SourcePath" }

$exists = (& sqlcmd -S $Server -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT IIF(DB_ID(N'HotelManagement') IS NULL, 0, 1);" -b).Trim()
if ($exists -eq '1') {
    if (-not $Reset) { throw "Database HotelManagement already exists. Run again with -Reset to recreate it." }
    & sqlcmd -S $Server -E -C -d master -Q "ALTER DATABASE HotelManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE HotelManagement;" -b
}

# Fix two errors in the supplied schema: the comma before PK_CTTN and CTDV.DonGia used by a trigger.
$sql = Get-Content -LiteralPath $SourcePath -Raw -Encoding UTF8
$sql = [regex]::Replace($sql, '("DaXoa" BIT DEFAULT 0)\s*(CONSTRAINT PK_CTTN)', '$1,' + [Environment]::NewLine + '    $2')
$sql = [regex]::Replace($sql, '(?m)^(\s*"SL" INT NOT NULL,\r?\n)(\s*"DaXoa" BIT DEFAULT 0,)', {
    param($m) $m.Groups[1].Value + '    "DonGia" MONEY DEFAULT 0,' + [Environment]::NewLine + $m.Groups[2].Value
})

$temp = Join-Path ([IO.Path]::GetTempPath()) "HotelManagement-$([guid]::NewGuid().ToString('N')).sql"
try {
    [IO.File]::WriteAllText($temp, $sql, [Text.UTF8Encoding]::new($false))
    & sqlcmd -S $Server -E -C -I -f 65001 -i $temp -b
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd could not create the database (exit code $LASTEXITCODE)." }
    & sqlcmd -S $Server -E -C -d HotelManagement -Q "UPDATE NhanVien SET TenNV = N'Hu' + NCHAR(0x1EF3) + N'nh V' + NCHAR(0x0169) + N' Huy Khang' WHERE MaNV = 'AD001';" -b
    & sqlcmd -S $Server -E -C -d HotelManagement -f 65001 -Q "SELECT (SELECT COUNT(*) FROM TaiKhoan) AS TaiKhoan, (SELECT COUNT(*) FROM Phong) AS Phong, (SELECT COUNT(*) FROM LoaiPhong) AS LoaiPhong;" -b
}
finally {
    Remove-Item -LiteralPath $temp -Force -ErrorAction SilentlyContinue
}
