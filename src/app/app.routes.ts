import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { AboutComponent } from './pages/about/about.component';
import { ContactComponent } from './pages/contact/contact.component';
import { FormComponent } from '../form/form.component';
import { ImageformComponent } from './components/imageform/imageform.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },
  { path: 'form', component: FormComponent },
  {path:'image',component:ImageformComponent},
  { path: '**', redirectTo: '' }
];
