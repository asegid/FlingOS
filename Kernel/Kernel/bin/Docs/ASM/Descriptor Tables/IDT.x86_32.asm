﻿; BEGIN - Create IDT
; See MultibootSignature.x86_32.asm for memory allocations

; Set the Int1 handler
; Load handler address
mov dword eax, BasicDebug_InterruptHandler
; Set low address bytes into entry (index) 1 of the table
mov byte [_NATIVE_IDT_Contents + 8], al
mov byte [_NATIVE_IDT_Contents + 9], ah
; Shift the address right 16 bits to get the high address bytes
shr dword eax, 0x10
; Set the high address bytes
mov byte [_NATIVE_IDT_Contents + 14], al
mov byte [_NATIVE_IDT_Contents + 15], ah
; Set the code segment selector
mov word [_NATIVE_IDT_Contents + 10], 0x8
; Must always be 0
mov byte [_NATIVE_IDT_Contents + 12], 0x0
; Set the type and attributes: 0x8F =	   1111		0			00		1
;										Trap Gate	Always 0	DPL		Present
mov byte [_NATIVE_IDT_Contents + 13], 0x8F

; Set the Int3 handler
mov dword eax, BasicDebug_InterruptHandler
mov byte [_NATIVE_IDT_Contents + 24], al
mov byte [_NATIVE_IDT_Contents + 25], ah
shr dword eax, 0x10
mov byte [_NATIVE_IDT_Contents + 30], al
mov byte [_NATIVE_IDT_Contents + 31], ah
mov word [_NATIVE_IDT_Contents + 26], 0x8
mov byte [_NATIVE_IDT_Contents + 28], 0x0
mov byte [_NATIVE_IDT_Contents + 29], 0x8F

mov dword ebx, _NATIVE_IDT_Contents

mov dword eax, Interrupt0Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8

add ebx, 24
; Set remaining interrupt handlers
 
mov dword eax, Interrupt4Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt5Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt6Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt7Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt8Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt9Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt10Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt11Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword eax, Interrupt12Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8

; Skip 13 - Triple Faults occur after every IRet!  
add ebx, 8
  
mov dword eax, Interrupt14Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8

; Skip 15
add ebx, 8 

mov dword eax, Interrupt16Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
  
mov dword ebx, _NATIVE_IDT_Contents
add ebx, 992
mov dword eax, Interrupt124Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8F
add ebx, 8
 

mov dword [_NATIVE_IDT_Pointer + 2], _NATIVE_IDT_Contents
mov dword eax, _NATIVE_IDT_Pointer
lidt [eax]
; END - Create IDT
jmp SkipIDTHandlers

; BEGIN - Proper exception handlers (i.e. they use the exceptions mechanism)

Interrupt0Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DivideByZeroException_NAMEEND___

Interrupt4Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
 
Interrupt6Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___

Interrupt8Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DoubleFaultException_NAMEEND___

Interrupt12Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_StackException_NAMEEND___

Interrupt14Handler:
mov dword eax, CR2
push eax
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_PageFaultException_NAMEEND__System_UInt32_System_UInt32_
IRet

; END - Proper exception handlers 

; BEGIN - Message-only Interrupt Handlers
 
Interrupt5HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 053
Interrupt5Handler:
pushad
mov dword eax, Interrupt5HandlerMsg
jmp MessageOnlyInterruptHandler

Interrupt7HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 055
Interrupt7Handler:
pushad
mov dword eax, Interrupt7HandlerMsg
jmp MessageOnlyInterruptHandler
  
Interrupt9HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 057
Interrupt9Handler:
pushad
mov dword eax, Interrupt9HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt10HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 048
Interrupt10Handler:
pushad
mov dword eax, Interrupt10HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt11HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 049
Interrupt11Handler:
pushad
mov dword eax, Interrupt11HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt16HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 054
Interrupt16Handler:
pushad
mov dword eax, Interrupt16HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt124HandlerMsg db 13, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 050, 052
Interrupt124Handler:
pushad
mov dword eax, Interrupt124HandlerMsg
jmp MessageOnlyInterruptHandler

MessageOnlyInterruptHandler:

push dword ebp
mov dword ebp, esp

push dword eax
push dword 0x02
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_
add esp, 4

mov ecx, 0x0F0FFFFF
MessageOnlyInterruptHandler.delayLoop1:
	nop
loop MessageOnlyInterruptHandler.delayLoop1

pop dword ebp

popad
IRet

; END - Message-only Interrupt Handlers

SkipIDTHandlers: