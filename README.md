## 게시전 확인 사항

현재 작성된 함수 앱에 컨테이너 이름을 확인하십시오.

이미지 파일 최적화 대상 컨테이너 이름으로 변경한 후 Azure Functions App 으로 게시하십시오.

![Test 컨테이너 대상 함수 앱](./OptimizeImageTestFunction.cs)
![Sample 컨테이너 대상 함수 앱](./OptimizeImageSampleFunction.cs)

## 개발환경 구성

[Work with Azure Functions Core Tools](https://docs.microsoft.com/ko-kr/azure/azure-functions/functions-run-local?tabs=v4%2Cmacos%2Ccsharp%2Cportal%2Cbash#v2) 페이지의 Azure Functions 핵심 도구 설치 내용을 확인하고, 로컬 개발 플랫폼에 따라 필요한 도구를 설치합니다.

> 가능하면 가장 최근 버전을 설치하십시오. 

### MacOS 의 경우

아래 명령을 실행해서 Azure Functions 핵심 도구를 설치할 수 있습니다.

```bash 
$ brew tap azure/functions
$ brew install azure-functions-core-tools@4
# v4 이전의 다른 버전 Azure Functions 핵심도구가 설치되어 있는 경우, 
# 아래 명령으로 실행파일의 링크를 변경합니다.
$ brew link --overwrite azure-functions-core-tools@4
```

## 이미지 최적화 함수 앱

현재 프로젝트는 Azure Functions v4 기반으로 격리된 프로세스<small>dotnet isolated</small>로 동작합니다.

사용되는 BlobTrigger 형식은 Azure Blob Storage 의 특정 컨테이너에 파일이 추가될 때 실행됩니다.

함수 앱 하나로 컨테이너 하나를 처리합니다.

따라서, 이미지 최적화 대상 컨테이너 수 만큼 함수앱이 필요합니다.


### Azure Storage Account 

Azure Storage Account 리소스를 작성해서 확인하거나, Azure Storage Account 를 에뮬레이션하는 도구를 사용해서 개발을 진행할 수 있습니다.

[로컬 Azure Storage 개발에 Azurite 에뮬레이터 사용](https://docs.microsoft.com/ko-kr/azure/storage/common/storage-use-azurite?tabs=npm) 페이지를 참조하세요.

[Azure Storage Explorer 도구](https://azure.microsoft.com/en-us/features/storage-explorer/)를 사용하면, 편리하게 파일을 추가, 삭제할 수 있습니다.

[Azure Storage Explorer 도구](https://azure.microsoft.com/en-us/features/storage-explorer/)는 Azure Storage Account, Azurite 으로 에뮬레이션 되는 환경도 연결할 수 있습니다.

> [How to use Azurite](./HOW-TO-USE-AZURITE.md) 페이지의 내용을 확인하십시오.


### 로컬 구성 파일 

프로젝트파일과 동일한 디렉터리에 `local.settings.json` 파일을 추가하고, 아래 내용을 입렵합니다.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureWebJobsSecretStorageType": "files",
    "BlobStorage": "<최적화 대상 이미지 파일이 관리되는 Azure Blob Storage 연결 문자열>",
    "Width": 1240,
    "Height": 940
  }
}
```
> Width, Height 항목은 이미지 파일 크기 조정 기준으로 사용됩니다.
> 
> Width 항목이 입력되지 않으면 1240 이 사용됩니다.
> 
> Height 항목이 입력되지 않으면 940 이 사용됩니다.

### 함수 앱 시작 (로컬)

터미널을 열고 프로젝트 파일이 있는 위치에서 아래 명령을 실행합니다.

```bash
$ cd path-to-project
$ func start
# ................
# ... 빌드 메시지 ...
# ................

Azure Functions Core Tools
Core Tools Version:       4.0.4544 Commit hash: N/A  (64-bit)
Function Runtime Version: 4.3.2.18186


Functions:

	OptimizeImageFileFunction: blobTrigger
```

### 함수 앱 추가

함수 앱을 추가하려면 아래 명령으로 필요한 코드를 스캐폴딩할 수 있습니다.

```bash
$ cd path-to-project
$ func new --name "함수 이름" 
```

> 이름정책 OptimaizeImage<컨테이너 이름>Function
> 예) 컨테이너 이름이 Sample 인 경우, `OptimizeImageSampleFunction` 을 사용합니다.

```bash
$ cd path-to-project
$ func new --name OptimizeImageSampleFunction
Select a number for template:
1. QueueTrigger
2. HttpTrigger
3. BlobTrigger
4. TimerTrigger
5. EventHubTrigger
6. ServiceBusQueueTrigger
7. ServiceBusTopicTrigger
8. EventGridTrigger
9. CosmosDBTrigger
Choose option: 3
Function name: OptimizeImageSampleFunction

The function "OptimizeImageSampleFunction" was created successfully from the "BlobTrigger" template.
```