// Fill out your copyright notice in the Description page of Project Settings.


#include "Chat/ChatWidget.h"

#include "Blueprint/WidgetTree.h"
#include "Engine/Font.h"
#include "Components/Button.h"
#include "Components/ScrollBox.h"
#include "Components/TextBlock.h"
#include "Player/SwarmPlayerController.h"

void UChatWidget::NativeConstruct()
{
    Super::NativeConstruct();

    CurrentChatType = EMsgType::All;
    ChatFont = LoadObject<UFont>(nullptr, TEXT("/Game/BP/Font/ko_font"));
    if (ChatFont == nullptr)
    {
        ChatFont = LoadObject<UFont>(nullptr, TEXT("/Engine/EngineFonts/Roboto"));
    }
    IsChatActive = false;
    
    // 버튼 클릭 이벤트 바인딩
    if (AllTabButton)
    {
        AllTabButton->OnClicked.AddDynamic(this, &UChatWidget::OnAllTabClicked);
    }
    if (GeneralTabButton)
    {
        GeneralTabButton->OnClicked.AddDynamic(this, &UChatWidget::OnGeneralTabClicked);
    }
    if (LocalTabButton)
    {
        LocalTabButton->OnClicked.AddDynamic(this, &UChatWidget::OnLocalTabClicked);
    }
    if (SystemTabButton)
    {
        SystemTabButton->OnClicked.AddDynamic(this, &UChatWidget::OnSystemTabClicked);
    }
    // 채팅 입력 이벤트 바인딩
    if (ChatInputBox)
    {
        ChatInputBox->OnTextCommitted.AddDynamic(this, &UChatWidget::OnChatInputCommitted);
    }
}

FReply UChatWidget::NativeOnKeyDown(const FGeometry& InGeometry, const FKeyEvent& InKeyEvent)
{
    // 입력창 포커스
    if (InKeyEvent.GetKey() == EKeys::Enter && IsChatActive == false)
    {
        FocusInputField();
        return FReply::Handled();
    }
    // 포커스 해제
    if (InKeyEvent.GetKey() == EKeys::Escape && IsChatActive)
    {
        UnFocusInputField();
        return FReply::Handled();
    }

    return Super::NativeOnKeyDown(InGeometry, InKeyEvent);
}

// 채팅 메시지 추가
void UChatWidget::AddChatMessage(const FChatMessage& Message)
{
    if (ChatScrollBox == nullptr || ChatMessageContainer == nullptr)
    {
        return;
    }

    // 메시지 포맷
    const FLinearColor MessageColor = GetChatTypeColor(Message.MsgType);
    const FString ChatPrefix = GetChatTypePrefix(Message.MsgType);
    const FDateTime MessageTime = FDateTime::FromUnixTimestamp(Message.Timestamp / 1000);
    const FDateTime KoTime = MessageTime + FTimespan::FromHours(9);
    const FString TimeString = FString::Printf(TEXT("[%02d:%02d]"), KoTime.GetHour(), KoTime.GetMinute());
    const FString FullMessage = FString::Printf(TEXT("%s %s %s: %s"), 
        *TimeString, 
        *ChatPrefix, 
        *Message.PlayerName, 
        *Message.Message);
    
    // 위젯 생성 후 스크롤박스 내 삽입
    if (UTextBlock* MessageText = WidgetTree->ConstructWidget<UTextBlock>())
    {
        MessageText->SetText(FText::FromString(FullMessage));
        MessageText->SetColorAndOpacity(FSlateColor(MessageColor));
        MessageText->SetAutoWrapText(true);

        FSlateFontInfo FontInfo = MessageText->GetFont();
        FontInfo.FontObject = ChatFont;
        FontInfo.Size = 14;
        MessageText->SetFont(FontInfo);
        
        ChatMessageContainer->AddChild(MessageText);
        GetWorld()->GetTimerManager().SetTimerForNextTick([this]()
        {
            if (ChatScrollBox)
            {
                ChatScrollBox->ScrollToEnd();
            }
        });
    }
}

// 채팅 타입 변경
void UChatWidget::SetCurrentChatType(const EMsgType NewChatType)
{
    CurrentChatType = NewChatType;
}

// 채팅창에 포커스
void UChatWidget::FocusInputField()
{
    if (ChatInputBox)
    {
        ChatInputBox->SetUserFocus(GetOwningPlayer());
        IsChatActive = true;
    }
}

// 포커스 해제
void UChatWidget::UnFocusInputField()
{
    if (ChatInputBox)
    {
        ChatInputBox->SetText(FText::GetEmpty());
        IsChatActive = false;
    }
}

void UChatWidget::OnAllTabClicked()
{
    SetCurrentChatType(EMsgType::All);
}

void UChatWidget::OnGeneralTabClicked()
{
    SetCurrentChatType(EMsgType::General);
}

void UChatWidget::OnLocalTabClicked()
{
    SetCurrentChatType(EMsgType::Local);
}

void UChatWidget::OnSystemTabClicked()
{
    SetCurrentChatType(EMsgType::System);
}

// 채팅 입력 완료 이벤트
void UChatWidget::OnChatInputCommitted(const FText& Text, const ETextCommit::Type CommitMethod)
{
    if (CommitMethod == ETextCommit::OnEnter)
    {
        const FString FullMessage = Text.ToString().TrimStartAndEnd();
        if (FullMessage.IsEmpty() == false)
        {
            // 메시지 전달 & 패킷 전송
            if (ASwarmPlayerController* PlayerController = Cast<ASwarmPlayerController>(GetOwningPlayer()))
            {
                PlayerController->SendChatMessage(FullMessage, CurrentChatType);
            }
        }
        // 포커스, 텍스트 초기화
        ChatInputBox->SetUserFocus(GetOwningPlayer());
        ChatInputBox->SetText(FText::GetEmpty());
    }
}

// 채팅 타입별 색상 가져오기
FLinearColor UChatWidget::GetChatTypeColor(EMsgType ChatType) const
{
    switch (ChatType)
    {
    case EMsgType::General:
        return FLinearColor::White;
    case EMsgType::Local:
        return FLinearColor::Gray;
    case EMsgType::System:
        return FLinearColor::Yellow;
    default:
        return FLinearColor::White;
    }
}

// 채팅 타입별 접두사 가져오기
FString UChatWidget::GetChatTypePrefix(const EMsgType ChatType) const
{
    switch (ChatType)
    {
    case EMsgType::General:
        return TEXT("[일반]");
    case EMsgType::Local:
        return TEXT("[지역]");
    case EMsgType::System:
        return TEXT("[시스템]");
    default:
        return TEXT("[모두]");
    }
}