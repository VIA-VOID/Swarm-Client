// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "ChatDefine.h"
#include "Protocol.pb.h"
#include "Blueprint/UserWidget.h"
#include "Components/Button.h"
#include "Components/EditableText.h"
#include "Components/ScrollBox.h"
#include "Components/VerticalBox.h"
#include "ChatWidget.generated.h"

/*-------------------------------------------------------
                    UChatWidget

- 채팅창 위젯
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API UChatWidget : public UUserWidget
{
	GENERATED_BODY()
    
public:
    virtual void NativeConstruct() override;
    virtual FReply NativeOnKeyDown(const FGeometry& InGeometry, const FKeyEvent& InKeyEvent) override;

    // 채팅 메시지 추가
    void AddChatMessage(const Protocol::SC_CHAT_MSG& InMsg);
    // 채팅창에 포커스
    void FocusInputField();
    // 포커스 해제
    void UnFocusInputField();
    
private:
    // 버튼 클릭 이벤트
    UFUNCTION()
    void OnAllTabClicked();
    UFUNCTION()
    void OnGeneralTabClicked();
    UFUNCTION()
    void OnLocalTabClicked();
    UFUNCTION()
    void OnSystemTabClicked();
    // 채팅 입력 완료 이벤트
    UFUNCTION()
    void OnChatInputCommitted(const FText& Text, const ETextCommit::Type CommitMethod);
    // 채팅 타입 변경
    void SetCurrentChatType(const EMsgType NewChatType);
    // 채팅 타입별 색상 가져오기
    FLinearColor GetChatTypeColor(const Protocol::MsgType MsgType) const;

protected:
    // 채팅 - 전체 버튼
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UButton* AllTabButton;
    // 채팅 - 일반 버튼
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UButton* GeneralTabButton;
    // 채팅 - 지역 버튼
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UButton* LocalTabButton;
    // 채팅 - 시스템 버튼
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UButton* SystemTabButton;
    // 채팅 - 스크롤박스
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UScrollBox* ChatScrollBox;
    // 채팅 - 스크롤박스 내 채팅영역
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UVerticalBox* ChatMessageContainer;
    // 채팅 - 채팅창
    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
    UEditableText* ChatInputBox;
    // 현재 선택된 채팅 타입
    UPROPERTY(BlueprintReadWrite, Category = "Chat")
    EMsgType CurrentChatType;
    // 폰트
    UPROPERTY()
    UFont* ChatFont;
    // 채팅창 활성화 여부
    bool IsChatActive;
};
