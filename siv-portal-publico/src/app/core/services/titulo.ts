import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';

export const MARCA = 'Quisqueya Flight Hub';

/**
 * Arma el título de la pestaña como "Página · Quisqueya Flight Hub", igual que
 * el _Layout de la intranet, para que las dos aplicaciones se vean como un
 * mismo sistema. El texto de cada página viene del `title` de la ruta.
 */
@Injectable({ providedIn: 'root' })
export class TituloQuisqueya extends TitleStrategy {
  constructor(private readonly title: Title) {
    super();
  }

  override updateTitle(estado: RouterStateSnapshot): void {
    const pagina = this.buildTitle(estado);
    this.title.setTitle(pagina ? `${pagina} · ${MARCA}` : MARCA);
  }
}
