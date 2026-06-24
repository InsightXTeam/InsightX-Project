import { Component, inject } from '@angular/core';
import { ReportService } from '../../services/report-service';

@Component({
  selector: 'app-report-upload',
  imports: [],
  templateUrl: './report-upload.html',
  styleUrl: './report-upload.css',
})



export class ReportUpload {


private service =
inject(ReportService);



selectedFile?:File;



onFileSelected(event:any){

this.selectedFile =
event.target.files[0];

}



upload(){


if(!this.selectedFile)
return;



this.service
.upload(this.selectedFile)
.subscribe({

next:(res)=>{

console.log(res);

alert('Uploaded');

},


error:(err)=>{

console.log(err);

}


});


}


}