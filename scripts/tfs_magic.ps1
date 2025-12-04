# Път към TFS Collection
$collectionUrl = "https://pacific/tfs/defaultcollection"

# 1. Изпълняваме tf dir и взимаме имената на папките
$folders = tf dir "$/" /collection:$collectionUrl |
    Select-String '^\s*\$' | 
    ForEach-Object {
        $_.ToString().Trim().TrimEnd("/") -replace '^\$', ''
    }

# Филтър: оставяме само папки (премахваме празни)
$folders = $folders | Where-Object { $_ -ne "" } | Where-Object { $_ -ne ":" }

# 2. Проверка дали simple.slnx съществува
$slnxSource = Join-Path (Get-Location) "simple.slnx"
if (-not (Test-Path $slnxSource)) {
    Write-Host "Грешка: simple.slnx не е намерен в текущата директория." -ForegroundColor Red
    exit 1
}

# 3. Обработка на всяка папка
foreach ($folder in $folders) {
 Write-Host $folder
    # Създаваме подпапка с името от TFS
    $targetDir = Join-Path (Get-Location) $folder
    if (-not (Test-Path $targetDir)) {
        New-Item -Path $targetDir -ItemType Directory | Out-Null
    }

    # 4. Копираме simple.slnx в новата папка
    $targetFile = Join-Path $targetDir ($folder + ".slnx")
    Copy-Item -Path $slnxSource -Destination $targetFile -Force

    Write-Host "Създадена папка: $folder и файл: $($folder).slnx"
}

Write-Host "Готово!" -ForegroundColor Green
