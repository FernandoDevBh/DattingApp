import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messsage',
  templateUrl: './member-messsage.component.html',
  styleUrls: ['./member-messsage.component.css']
})
export class MemberMesssageComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() messages: Message[] = [];
  @Input() username: string;
  messageContent: string;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void { }

  sendMessage() {
    this.messageService.sendMessage(this.username, this.messageContent).subscribe((message: Message) => {
      this.messages.push(message);
      this.messageForm.reset();
    });
  }
}
