import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './shared/navbar/navbar';
import { NotificacionToast } from './shared/notificacion-toast/notificacion-toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, NotificacionToast],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
