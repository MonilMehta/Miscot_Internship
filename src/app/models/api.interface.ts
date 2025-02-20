export interface ApiResponse {
  data: string[][];
}

export interface Field {
  reffieldName: string;
  refName: string;
  displayType: string;
  fieldType: string;
  displayIndex: string;
}

export interface Section {
  label: Field;
  fields: Field[];
  isCollapsed: boolean;
}
