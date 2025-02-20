import { Component, OnInit, HostListener, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfigService } from '../app/services/config.service';
import { ApiResponse, Field, Section } from '../app/models/api.interface';

@Component({
  selector: 'app-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css']
})
export class FormComponent implements OnInit, AfterViewInit {
  sections: Section[] = [];
  form: FormGroup;
  dropdownOptions: { [key: string]: string[] } = {
    Application_Type: ['New', 'Renewal', 'Modification'],
    Account_Type: ['Savings', 'Current', 'Fixed Deposit'],
    Title: ['Mr', 'Mrs', 'Ms', 'Dr'],
    Gender: ['Male', 'Female', 'Other'],
    Occupation: ['Service', 'Business', 'Student', 'Other'],
    MaritalStatus: ['Single', 'Married', 'Divorced', 'Widowed']
  };
  currentSectionId: string = '';

  constructor(private configService: ConfigService, private fb: FormBuilder) {
    this.form = this.fb.group({});
  }

  ngOnInit() {
    this.fetchFields();
  }

  ngAfterViewInit() {
    // Trigger update when view is ready
    this.updateCurrentSection();
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.updateCurrentSection();
  }

  updateCurrentSection() {
    // Check each section header element's position relative to viewport
    for (let section of this.sections) {
      const elem = document.getElementById(section.label.reffieldName);
      if (elem) {
        const rect = elem.getBoundingClientRect();
        // Consider sections whose top is less than 100px from top as current
        if (rect.top >= 0 && rect.top < 100) {
          this.currentSectionId = section.label.reffieldName;
          break;
        }
      }
    }
  }

  fetchFields() {
    this.configService.getFields().subscribe(
      (response: ApiResponse) => {
        if (response && response.data) {
          const fields = response.data.map(record => {
            const field = this.parseField(record);
            // console.log('Parsed Field:', {
            //   name: field.refName,
            //   type: field.fieldType,
            //   displayType: field.displayType
            // });
            return field;
          });
          this.organizeSections(fields);
          this.createFormControls();
        } else {
          console.error('Invalid response structure:', response);
        }
      },
      error => {
        console.error('Error fetching fields:', error);
      }
    );
  }

  organizeSections(fields: Field[]) {
    let currentSection: Section | null = null;

    fields.forEach(field => {
      if (field.fieldType.toLowerCase() === 'label') { // Changed from displayType to fieldType
        // console.log('Found Section Label:', field.refName);
        // If we find a label, start a new section
        currentSection = {
          label: field,
          fields: [],
          isCollapsed: false
        };
        this.sections.push(currentSection);
      } else if (currentSection) {
        // Add field to current section
        if(field.displayType=='combo' && field.fieldType!='text'){
          console.log(field.reffieldName)
        }
        currentSection.fields.push(field);
      }
    });

    // Debug output
    this.sections.forEach(section => {
//      console.log(`Section "${section.label.refName}" has ${section.fields.length} fields`);
    });
  }

  createFormControls() {
    this.sections.forEach(section => {
      section.fields.forEach(field => {
        const validators = field.displayType === 'Text' ? [Validators.required] : [];
        this.form.addControl(field.reffieldName, this.fb.control('', validators));
      });
    });
  }

  toggleSection(section: Section) {
    section.isCollapsed = !section.isCollapsed;
  }

  parseField(item: string[]): Field {
    const field: any = {};
    item.forEach(str => {
      const [key, ...rest] = str.split(':');
      const value = rest.join(':').trim();
      // Keep the original casing for the property names to match the interface
      const prop = key.trim();
      switch (prop) {
        case 'ReffieldName':
          field.reffieldName = value;
          break;
        case 'RefName':
          field.refName = value;
          break;
        case 'DisplayType':
          field.displayType = value;
          break;
        case 'FieldType':
          field.fieldType = value;
          break;
        case 'DisplayIndex':
          field.displayIndex = value;
          break;
      }
    });
    return field as Field;
  }

  getInputType(field: Field): string {
    switch (field.fieldType.toLowerCase()) {
      case 'datetime':
        return 'date';
      case 'date':
        return 'date';
      case 'number':
      case 'numeric':
        return 'number';
      case 'varchar':
      default:
        return 'text';
    }
  }

  navigateTo(field: Field) {
    const element = document.getElementById(field.reffieldName);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }

  get progressPercentage(): number {
    const keys = Object.keys(this.form.controls);
    if (keys.length === 0) { return 0; }
    const validCount = keys.filter(key => this.form.get(key)?.valid).length;
    return Math.floor((validCount / keys.length) * 100);
  }

  onSubmit() {
    console.log('Submit clicked, form valid:', this.form.valid);
    if (!this.form.valid) {
      console.log('Form is invalid:', this.form.errors);
      return;
    }
    console.log('Form Data:', JSON.stringify(this.form.value));
  }
  
}