import { Routes } from '@angular/router';
import { ChatPage } from './chat/chat-page';
import { AssessPage } from './assess/assess-page';

export const routes: Routes = [
  { path: '', component: ChatPage, title: 'Chat · CreditAssist' },
  { path: 'assess', component: AssessPage, title: 'Assess loan · CreditAssist' },
  { path: '**', redirectTo: '' }
];