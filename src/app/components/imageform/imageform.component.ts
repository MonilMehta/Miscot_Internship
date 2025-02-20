import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface UploadedFile {
  id: string;
  file: File;
  preview: string;
  type: string;
}

@Component({
  selector: 'app-imageform',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './imageform.component.html',
  styleUrls: ['./imageform.component.css']
})
export class ImageformComponent {
  uploadedFiles: UploadedFile[] = [];
  isDragging = false;
  errorMessage = '';

  private generateId(): string {
    return Math.random().toString(36).substring(2) + Date.now().toString(36);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    
    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(files);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(input.files);
      // Clear input value to allow selecting the same file again
      input.value = '';
    }
  }

  private handleFiles(files: FileList): void {
    Array.from(files).forEach(file => {
      if (!this.isValidFileType(file)) {
        this.errorMessage = `File ${file.name} is not a supported format. Please use JPEG, JPG, or PDF files.`;
        return;
      }

      const fileData: UploadedFile = {
        id: this.generateId(),
        file,
        preview: '',
        type: file.type.startsWith('image/') ? 'image' : 'pdf'
      };

      if (file.type.startsWith('image/')) {
        this.createImagePreview(fileData);
      } else {
        // For PDFs, just add to uploaded files
        this.uploadedFiles.push(fileData);
      }
    });
  }

  private isValidFileType(file: File): boolean {
    const allowedTypes = ['image/jpeg', 'image/jpg', 'application/pdf'];
    return allowedTypes.includes(file.type);
  }

  private createImagePreview(fileData: UploadedFile): void {
    const reader = new FileReader();
    
    reader.onload = (e: ProgressEvent<FileReader>) => {
      fileData.preview = e.target?.result as string;
      this.uploadedFiles.push(fileData);
    };

    reader.onerror = () => {
      this.errorMessage = `Error reading file: ${fileData.file.name}`;
    };

    reader.readAsDataURL(fileData.file);
  }

  previewFile(file: UploadedFile): void {
    if (file.type === 'pdf') {
      const url = URL.createObjectURL(file.file);
      window.open(url, '_blank');
      // Clean up the URL object
      URL.revokeObjectURL(url);
    } else if (file.type === 'image') {
      window.open(file.preview, '_blank');
    }
  }

  deleteFile(fileId: string): void {
    this.uploadedFiles = this.uploadedFiles.filter(file => file.id !== fileId);
  }

  clearError(): void {
    this.errorMessage = '';
  }
}
